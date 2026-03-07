

using System.Diagnostics;
using EBayCloneAPI.Services;

namespace EBayCloneAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
private readonly ILogger<PaymentService> _logger;
        public PaymentService(IEnumerable<IPaymentProvider> providers, ILogger<PaymentService> logger)
        {
            _providers = providers;
            _logger = logger;
        }
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
        public IPaymentProvider GetProvider(string method)
        {
            var provider = _providers.FirstOrDefault(p =>
                p.Method.Equals(method, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
                throw new Exception($"Payment method {method} not supported");

            return provider;
        }
    }
}
