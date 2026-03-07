namespace EBayCloneAPI.Services
{
    public interface IPaymentService
    {
        IPaymentProvider GetProvider(string method);
        Task<(bool success, string transactionId)> PayAsync(string method, decimal amount, string authToken, string secureKey);
    }
}
