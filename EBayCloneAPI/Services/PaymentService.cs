using System.Diagnostics;
using EBayAPI.Models.Hooks;

namespace EBayCloneAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly IEnumerable<IPaymentEventHook> _paymentHooks;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IEnumerable<IPaymentProvider> providers,
            IEnumerable<IPaymentEventHook> paymentHooks,
            ILogger<PaymentService> logger)
        {
            _providers = providers;
            _paymentHooks = paymentHooks;
            _logger = logger;
        }

        public async Task<(bool success, string transactionId)> PayAsync(
            string method,
            decimal amount,
            string authToken,
            string secureKey)
        {
            var sw = Stopwatch.StartNew();

            // auth check
            if (string.IsNullOrEmpty(authToken) || secureKey != "SECURE_KEY_123")
            {
                _logger.LogWarning("Payment auth failed");

                foreach (var hook in _paymentHooks)
                {
                    await hook.OnPaymentFailed(null);
                }

                return (false, string.Empty);
            }

            // simulate payment latency
            var simulated = method == "PayPal" ? 800 : 200;
            await Task.Delay(simulated);

            sw.Stop();

            _logger.LogInformation(
                "Payment processed in {ms}ms for {method}",
                sw.ElapsedMilliseconds,
                method);

            var transactionId = Guid.NewGuid().ToString();

            // Trigger success hooks
            foreach (var hook in _paymentHooks)
            {
                await hook.OnPaymentSuccess(null);
            }

            return (true, transactionId);
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