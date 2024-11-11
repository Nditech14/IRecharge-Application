using Application.DTO.RequestDto;
using Application.DTO.ResponseDto;
using Application.Service.Abstraction;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Untilities.CachManager;
using Infrastructure.Untilities.Communication.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.Service.Implementation
{
    public class BillService : IBillService
    {
        private readonly ICosmosDbService<Bill> _cosmosDbServiceBill;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BillService> _logger;
        private readonly IRedisCacheManager _redisCacheManager;
        private readonly IWalletService _walletService;

        public BillService(
            IRedisCacheManager redisCacheManager,
            IWalletService walletService,
            ICosmosDbService<Bill> cosmosDbServiceBill,
            IMapper mapper,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BillService> logger)
        {
            _redisCacheManager = redisCacheManager;
            _walletService = walletService;
            _cosmosDbServiceBill = cosmosDbServiceBill;
            _mapper = mapper;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string GetUserId() => _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
        private string GetUserEmail() => _httpContextAccessor.HttpContext?.Items["Email"]?.ToString();
        private string GetUserFullName() => _httpContextAccessor.HttpContext?.Items["FullName"]?.ToString();

        public async Task<BillResponseDto> CreateBillAsync(CreateBillRequest request)
        {
            var emailList = await _walletService.GetAllActiveWalletEmailsAsync();

            var bill = new Bill
            {
                id = Guid.NewGuid().ToString(),
                Amount = request.Amount,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _cosmosDbServiceBill.AddItemAsync(bill);
            await _redisCacheManager.SetItemAsync($"bill:{bill.id}", bill, TimeSpan.FromHours(1));

            foreach (var email in emailList)
            {
                await _emailService.SendEmailAsync(email, "New Bill Created",
                    $"Dear Customer,\n\nA new bill of amount {bill.Amount:C} has been created.",
                    null);
            }

            return new BillResponseDto
            {
                BillId = bill.id,
                Status = bill.Status,
                Amount = bill.Amount,
                Message = "Bill created and notifications sent successfully."
            };
        }

        public async Task<BillResponseDto> PayBillAsync(string id)
        {
            var userId = GetUserId();
            var email = GetUserEmail();
            var fullName = GetUserFullName();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return new BillResponseDto { Message = "User not authenticated.", Status = PaymentStatus.Failed };

            var bill = await _redisCacheManager.GetItemAsync<Bill>($"bill:{id}");

            if (bill == null)
                bill = await _cosmosDbServiceBill.GetItemAsync(id, id);

            if (bill == null)
                return new BillResponseDto { Message = "Bill not found.", Status = PaymentStatus.Failed };

            if (bill.Status == PaymentStatus.Paid)
                return new BillResponseDto { Message = "Bill already paid.", Status = PaymentStatus.Failed };

            var paymentResponse = await _walletService.RemoveFundsAsync(bill.Amount);
            if (paymentResponse.StatusCode != 200)
            {
                bill.Status = PaymentStatus.Failed;
                await _cosmosDbServiceBill.UpdateItemAsync(bill.id, bill, id);

                await _emailService.SendEmailAsync(email, "Payment Failed",
                    $"Dear {fullName},\n\nPayment for bill {bill.id} failed due to insufficient funds.",
                    null);

                return new BillResponseDto { Message = "Insufficient wallet balance.", Status = PaymentStatus.Failed };
            }

            bill.Status = PaymentStatus.Paid;
            bill.PaidAt = DateTime.UtcNow;

            await _cosmosDbServiceBill.UpdateItemAsync(bill.id, bill, id);
            await _redisCacheManager.RemoveItemAsync($"bill:{id}");

            await _emailService.SendEmailAsync(email, "Payment Successful",
                $"Dear {fullName},\n\nYour payment of {bill.Amount:C} for bill {bill.id} was successful.",
                null);

            await _cosmosDbServiceBill.DeleteItemAsync(bill.id, id);

            return new BillResponseDto
            {
                BillId = bill.id,
                Status = bill.Status,
                Amount = bill.Amount,
                Message = "Payment successful."
            };
        }
    }
}
