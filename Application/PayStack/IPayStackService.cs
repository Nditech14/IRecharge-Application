
namespace Application.PayStack
{
    public interface IPayStackService
    {
        Task<PaystackData> CreateTransactionAsync(decimal amount, string email, string reference);
        Task<bool> VerifyTransactionAsync(string reference);
    }
}