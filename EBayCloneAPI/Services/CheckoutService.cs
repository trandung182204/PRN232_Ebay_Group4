using EBayAPI.DTOs;
using EBayAPI.Models.Hooks;

namespace EBayAPI.Services
{
    public class CheckoutService
    {
        private readonly PluginManager _pluginManager;

        public CheckoutService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public async Task Checkout(string paymentMethod, PaymentRequest request)
        {
            var paymentPlugin = _pluginManager.GetPayment(paymentMethod);

            if (paymentPlugin == null)
                throw new Exception("Payment method not found");

            var result = await paymentPlugin.ProcessPaymentAsync(request);

            if (!result.Success)
                throw new Exception("Payment failed");
        }
    }
}
