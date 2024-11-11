using Application.DTO.ResponseDto;
using Infrastructure.Untilities.Common;

namespace Application.Service.Abstraction
{
    public interface IWalletService
    {
        Task<ApiResponse<AddFundsResponseDto>> AddFundsAsync(decimal amount);
        Task<ApiResponse<ConfirmPaymentResponseDto>> ConfirmPaymentAsync(string reference);
        Task<ApiResponse<WalletResponseDto>> CreateWalletAsync();
        Task<ApiResponse<WalletStatusResponseDto>> DeactivateWalletAsync();
        Task<ApiResponse<WalletBalanceResponseDto>> GetWalletBalanceAsync();
        Task<ApiResponse<WalletStatusResponseDto>> ReactivateWalletAsync();
        Task<ApiResponse<RemoveFundsResponseDto>> RemoveFundsAsync(decimal amount);
        Task<IEnumerable<string>> GetAllActiveWalletEmailsAsync();

    }
}