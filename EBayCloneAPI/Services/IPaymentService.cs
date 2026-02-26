namespace EBayCloneAPI.Services
{
    public interface IPaymentService
    {
        Task<(bool success, string transactionId)> PayAsync(string method, decimal amount, string authToken, string secureKey);
    }
}
