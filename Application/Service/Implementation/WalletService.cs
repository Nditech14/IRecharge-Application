using Application.Service.Abstraction;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Untilities.CachManager;
using Infrastructure.Untilities.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Application.DTO.ResponseDto;
using Application.PayStack;
using Application.PayStcak;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Infrastructure.Untilities.Communication.Abstraction;

namespace Application.Service.Implementation
{
    public class WalletService : IWalletService
    {
        private readonly ICosmosDbService<Wallet> _cosmosDbServiceWallet;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisCacheManager _redisCacheManager;
        private readonly ILogger<WalletService> _logger;
        private readonly IPayStackService _payStackService;
        private readonly PayStackSettings _payStackSettings;
        private readonly IConfiguration _configuration;

        private const int CacheExpiryMinutes = 10;
        private readonly decimal _lowBalanceThreshold;

        public WalletService(
            ICosmosDbService<Wallet> cosmosDbServiceWallet,
            IMapper mapper,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IRedisCacheManager redisCacheManager,
            ILogger<WalletService> logger,
            IPayStackService payStackService,
            PayStackSettings payStackSettings,
            IConfiguration configuration)
        {
            _cosmosDbServiceWallet = cosmosDbServiceWallet;
            _mapper = mapper;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _redisCacheManager = redisCacheManager;
            _logger = logger;
            _payStackService = payStackService;
            _payStackSettings = payStackSettings;
            _configuration = configuration;
            _lowBalanceThreshold = _configuration.GetValue<decimal>("WalletSettings:LowBalanceThreshold", 1000m); 
        }

        private string GetUserId() => _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
        private string GetUserEmail() => _httpContextAccessor.HttpContext?.Items["Email"]?.ToString();
        private string GetUserFullName() => _httpContextAccessor.HttpContext?.Items["FulName"]?.ToString();
        private string GetCacheKey(string userId) => $"wallet:{userId}";

        #region Create Wallet
        public async Task<ApiResponse<WalletResponseDto>> CreateWalletAsync()
        {
            var userId = GetUserId();
            var email = GetUserEmail();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("CreateWalletAsync: User is not authenticated.");
                return new ApiResponse<WalletResponseDto>(null, "User is not authenticated.", 401);
            }

            var existingWallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (existingWallet != null)
            {
                _logger.LogInformation($"CreateWalletAsync: Wallet already exists for userId {userId}.");
                return new ApiResponse<WalletResponseDto>(null, "Wallet already exists.", 409);
            }

            var wallet = new Wallet
            {
                UserId = userId,
                UserEmail = email,
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cosmosDbServiceWallet.AddItemAsync(wallet);
            await _redisCacheManager.RemoveItemAsync(GetCacheKey(userId));

            var responseDto = new WalletResponseDto
            {
                Id = wallet.id,
                UserId = wallet.UserId,
                UserEmail = wallet.UserEmail,
                Balance = wallet.Balance,
                IsActive = wallet.IsActive,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt
            };

            return new ApiResponse<WalletResponseDto>(responseDto, "Wallet created successfully.", 201);
        }

        #endregion

        #region Get Wallet Balance
        public async Task<ApiResponse<WalletBalanceResponseDto>> GetWalletBalanceAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetWalletBalanceAsync: User is not authenticated.");
                return new ApiResponse<WalletBalanceResponseDto>(null, "User is not authenticated.", 401);
            }
            var cacheKey = GetCacheKey(userId);
            var wallet = await _redisCacheManager.GetItemAsync<Wallet>(cacheKey);

            if (wallet == null)
            {
                wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    _logger.LogWarning($"GetWalletBalanceAsync: Wallet not found for userId {userId}.");
                    return new ApiResponse<WalletBalanceResponseDto>(null, "Wallet not found.", 404);
                }
                if (!wallet.IsActive)
                {
                    _logger.LogWarning($"GetWalletBalanceAsync: Wallet is inactive for userId {userId}.");
                    return new ApiResponse<WalletBalanceResponseDto>(null, "Wallet is inactive.", 403);
                }
                await _redisCacheManager.SetItemAsync(cacheKey, wallet, TimeSpan.FromMinutes(CacheExpiryMinutes));
            }

            var responseDto = new WalletBalanceResponseDto
            {
                Balance = wallet.Balance,
                Message = "Balance fetched successfully"
            };

            _logger.LogInformation($"GetWalletBalanceAsync: Balance fetched successfully for userId {userId}.");

            return new ApiResponse<WalletBalanceResponseDto>(responseDto, "Successfully fetched balance", 200);
        }

        #endregion

        #region Add Funds
        public async Task<ApiResponse<AddFundsResponseDto>> AddFundsAsync(decimal amount)
        {
            var userId = GetUserId();
            var email = GetUserEmail();
            var fullName = GetUserFullName();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("AddFundsAsync: User is not authenticated.");
                return new ApiResponse<AddFundsResponseDto>(null, "User is not authenticated.", 401);
            }

            var wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                _logger.LogWarning($"AddFundsAsync: Wallet not found for userId {userId}.");
                return new ApiResponse<AddFundsResponseDto>(null, "Wallet not found.", 404);
            }
            if (!wallet.IsActive)
            {
                _logger.LogWarning($"AddFundsAsync: Wallet is inactive for userId {userId}.");
                return new ApiResponse<AddFundsResponseDto>(null, "Wallet is inactive.", 403);
            }

            // Generate a unique reference for this transaction
            var reference = Guid.NewGuid().ToString();

            // Store transaction details in Redis with the reference as the key
            var transactionData = new
            {
                UserId = userId,
                Email = email,
                Amount = amount,
                WalletId = wallet.id,
                FullName = fullName
            };

            await _redisCacheManager.SetItemAsync($"transaction:{reference}", transactionData, TimeSpan.FromMinutes(30));

            var paystackResponse = await _payStackService.CreateTransactionAsync(amount, email, reference);
            if (paystackResponse == null)
            {
                _logger.LogError("AddFundsAsync: Unable to create Paystack transaction.");
                return new ApiResponse<AddFundsResponseDto>(null, "Unable to create Paystack transaction.", 500);
            }

            _logger.LogInformation($"AddFundsAsync: Transaction created successfully for userId {userId} with reference {reference}.");

            
            return new ApiResponse<AddFundsResponseDto>(new AddFundsResponseDto
            {
                Message = "Please complete payment.",
                WalletId = wallet.id,
                Balance = wallet.Balance,
                AuthorizationUrl = paystackResponse.authorization_url,
                Reference = reference
            }, null, 200);
        }
        #endregion


        #region Confirm Payment Funds
        public async Task<ApiResponse<ConfirmPaymentResponseDto>> ConfirmPaymentAsync(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                _logger.LogWarning("ConfirmPaymentAsync: Invalid payment reference.");
                return new ApiResponse<ConfirmPaymentResponseDto>(null, "Invalid payment reference.", 400);
            }

            
            var isValid = await _payStackService.VerifyTransactionAsync(reference);
            if (!isValid)
            {
                _logger.LogError($"ConfirmPaymentAsync: Payment verification failed for reference {reference}.");
                return new ApiResponse<ConfirmPaymentResponseDto>(null, "Payment verification failed or transaction not found.", 404);
            }

            
            var transactionData = await _redisCacheManager.GetItemAsync<JsonElement>($"transaction:{reference}");
            if (transactionData.ValueKind == JsonValueKind.Undefined || transactionData.ValueKind == JsonValueKind.Null)
            {
                _logger.LogError($"ConfirmPaymentAsync: Transaction data not found in cache for reference {reference}.");
                return new ApiResponse<ConfirmPaymentResponseDto>(null, "Transaction not found in cache.", 404);
            }


            var userId = transactionData.GetProperty("UserId").GetString();
            var amountPaid = transactionData.GetProperty("Amount").GetDecimal();
            var email = transactionData.GetProperty("Email").GetString();
            var fullName = transactionData.GetProperty("FullName").GetString();


           
            var wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (wallet == null || !wallet.IsActive)
            {
                _logger.LogWarning($"ConfirmPaymentAsync: Wallet not found or inactive for userId {userId}.");
                return new ApiResponse<ConfirmPaymentResponseDto>(null, "Wallet not found or inactive.", 404);
            }

            
            wallet.Balance += amountPaid;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _cosmosDbServiceWallet.UpdateItemAsync(wallet.id, wallet, userId);
            await _redisCacheManager.RemoveItemAsync(GetCacheKey(userId));
            await _redisCacheManager.RemoveItemAsync($"transaction:{reference}");

            _logger.LogInformation($"ConfirmPaymentAsync: Wallet balance updated for userId {userId}, new balance {wallet.Balance}.");

            #region WalletBalanceUpdatedEvent
            // await PublishEventAsync("wallet.balance.updated", new { UserId = wallet.UserId, NewBalance = wallet.Balance });

            #endregion
            await _emailService.SendEmailAsync(email, "Credit Alert",
                $"Dear {fullName},\n\nYour wallet has been credited with {amountPaid:C}. Your new balance is {wallet.Balance:C}.",
                null);

            _logger.LogInformation($"ConfirmPaymentAsync: Credit alert email sent to {email}.");



            var responseDto = new ConfirmPaymentResponseDto
            {
                WalletId = wallet.id,
                NewBalance = wallet.Balance,
                AmountAdded = amountPaid,
                Message = "Payment confirmed and wallet updated successfully."
            };
            return new ApiResponse<ConfirmPaymentResponseDto>(responseDto, "success", 200);
        }
        #endregion

        #region Remove Funds
        public async Task<ApiResponse<RemoveFundsResponseDto>> RemoveFundsAsync(decimal amount)
        {
            var userId = GetUserId();
            var email = GetUserEmail();
            var fullName = GetUserFullName();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("RemoveFundsAsync: User is not authenticated.");
                return new ApiResponse<RemoveFundsResponseDto>(null, "User is not authenticated.", 401);
            }

            var wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                _logger.LogWarning($"RemoveFundsAsync: Wallet not found for userId {userId}.");
                return new ApiResponse<RemoveFundsResponseDto>(null, "Wallet not found.", 404);
            }
            if (!wallet.IsActive)
            {
                _logger.LogWarning($"RemoveFundsAsync: Wallet is inactive for userId {userId}.");
                return new ApiResponse<RemoveFundsResponseDto>(null, "Wallet is inactive.", 403);
            }
            if (wallet.Balance < amount)
            {
                _logger.LogWarning($"RemoveFundsAsync: Insufficient funds for userId {userId}.");
                return new ApiResponse<RemoveFundsResponseDto>(null, "Insufficient funds.", 400);
            }

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _cosmosDbServiceWallet.UpdateItemAsync(wallet.id, wallet, wallet.UserId);
            await _redisCacheManager.RemoveItemAsync(GetCacheKey(userId));
            await _redisCacheManager.SetItemAsync(GetCacheKey(userId), wallet, TimeSpan.FromMinutes(CacheExpiryMinutes));

            _logger.LogInformation($"RemoveFundsAsync: Funds removed from wallet for userId {userId}, amount {amount}, new balance {wallet.Balance}.");

            #region WalletBalanceUpdatedEvent
            //  await PublishEventAsync("wallet.balance.updated", new { UserId = wallet.UserId, NewBalance = wallet.Balance });
            #endregion

            // Check for low balance
            if (wallet.Balance < _lowBalanceThreshold)
            {
                #region LowBalanceEvent
                //await PublishEventAsync("wallet.balance.low", new { UserId = wallet.UserId, Balance = wallet.Balance });
                #endregion
                // Send low balance notification
                await _emailService.SendEmailAsync(wallet.UserEmail, "Low Balance Alert",
                    $"Dear {fullName},\n\nYour wallet balance is low ({wallet.Balance:C}). Please add funds to avoid service interruption.",
                    null);

                _logger.LogInformation($"RemoveFundsAsync: Low balance notification sent to {wallet.UserEmail}.");
            }

            // Send debit alert email
            await _emailService.SendEmailAsync(wallet.UserEmail, "Debit Alert",
                $"Dear {fullName},\n\nYour wallet has been debited by {amount:C}. Your new balance is {wallet.Balance:C}.",
                null);

            var responseDto = new RemoveFundsResponseDto
            {
                WalletId = wallet.id,
                NewBalance = wallet.Balance,
                Message = "Funds removed successfully"
            };

            return new ApiResponse<RemoveFundsResponseDto>(responseDto, "success", 200);
        }

        #endregion

        #region Deactivate Wallet
        public async Task<ApiResponse<WalletStatusResponseDto>> DeactivateWalletAsync()
        {
            var userId = GetUserId();

            var wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                _logger.LogWarning($"DeactivateWalletAsync: Wallet not found for userId {userId}.");
                return new ApiResponse<WalletStatusResponseDto>(null, "Wallet not found.", 404);
            }

            wallet.IsActive = false;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _cosmosDbServiceWallet.UpdateItemAsync(wallet.id, wallet, wallet.UserId);
            await _redisCacheManager.RemoveItemAsync(GetCacheKey(userId));

            _logger.LogInformation($"DeactivateWalletAsync: Wallet deactivated for userId {userId}.");

            #region WalletDeactivatedEvent
            //await PublishEventAsync("wallet.deactivated", new { UserId = wallet.UserId });
            #endregion
            var responseDto = new WalletStatusResponseDto
            {
                WalletId = wallet.id,
                IsActive = wallet.IsActive,
                Message = "Wallet deactivated successfully"
            };

            return new ApiResponse<WalletStatusResponseDto>(responseDto, "success", 200);
        }
        #endregion

        #region Reactivate Wallet
        public async Task<ApiResponse<WalletStatusResponseDto>> ReactivateWalletAsync()
        {
            var userId = GetUserId();

            var wallet = await _cosmosDbServiceWallet.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                _logger.LogWarning($"ReactivateWalletAsync: Wallet not found for userId {userId}.");
                return new ApiResponse<WalletStatusResponseDto>(null, "Wallet not found.", 404);
            }

            wallet.IsActive = true;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _cosmosDbServiceWallet.UpdateItemAsync(wallet.id, wallet, wallet.UserId);
            await _redisCacheManager.RemoveItemAsync(GetCacheKey(userId));

            _logger.LogInformation($"ReactivateWalletAsync: Wallet reactivated for userId {userId}.");

            #region WalletReactivatedEvent
           // await PublishEventAsync("wallet.reactivated", new { UserId = wallet.UserId });
            #endregion
            var responseDto = new WalletStatusResponseDto
            {
                WalletId = wallet.id,
                IsActive = wallet.IsActive,
                Message = "Wallet reactivated successfully"
            };

            return new ApiResponse<WalletStatusResponseDto>(responseDto, "success", 200);
        }
        #endregion

        #region GetAllActiveWalletEmails
        public async Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync()
        {
            _logger.LogInformation("GetAllActiveWalletEmailsAsync: Fetching all active wallet emails.");
            return await _cosmosDbServiceWallet.GetAllActiveWalletEmailsAsync();

        }
        #endregion
    }
}
