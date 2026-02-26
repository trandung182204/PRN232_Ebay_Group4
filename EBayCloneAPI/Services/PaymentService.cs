using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(ILogger<PaymentService> logger) => _logger = logger;

        // Simulate payments (PayPal, COD). Verify authToken & secureKey.
        public async Task<(bool success, string transactionId)> PayAsync(string method, decimal amount, string authToken, string secureKey)
        {
            var sw = Stopwatch.StartNew();
            // Minimal auth check
            if (string.IsNullOrEmpty(authToken) || secureKey != "SECURE_KEY_123")
            {
                _logger.LogWarning("Payment auth failed");
                return (false, string.Empty);
            }

            // Simulate latency and ensure <= 2s
            var simulated = method == "PayPal" ? 800 : 200; // ms
            await Task.Delay(simulated);

            sw.Stop();
            _logger.LogInformation("Payment processed in {ms}ms for {method}", sw.ElapsedMilliseconds, method);

            return (true, Guid.NewGuid().ToString());
        }
    }
}
