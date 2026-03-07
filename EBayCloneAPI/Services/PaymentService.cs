

using EBayCloneAPI.Services;

namespace EBayAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _providers;

        public PaymentService(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers;
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
