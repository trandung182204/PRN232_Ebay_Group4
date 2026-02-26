namespace EBayCloneAPI.Services
{
    public interface IEmailService
    {
        Task SendPaymentConfirmationAsync(string toEmail, string orderId);
        Task SendOrderStatusChangeAsync(string toEmail, string orderId, string status);
    }
}
