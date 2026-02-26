using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        public EmailService(ILogger<EmailService> logger) => _logger = logger;

        public Task SendPaymentConfirmationAsync(string toEmail, string orderId)
        {
            _logger.LogInformation("[Email] Payment confirmation to {email} for order {orderId}", toEmail, orderId);
            // Simulate send
            return Task.CompletedTask;
        }

        public Task SendOrderStatusChangeAsync(string toEmail, string orderId, string status)
        {
            _logger.LogInformation("[Email] Order {orderId} status changed to {status}. Notify {email}", orderId, status, toEmail);
            return Task.CompletedTask;
        }
    }
}
