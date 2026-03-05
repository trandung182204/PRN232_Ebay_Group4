using EBayAPI.DTOs;

namespace EBayAPI.Models.Hooks
{
    public class StripePaymentPlugin : IPaymentHook
    {
        public string Name => "Stripe";

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // call Stripe API

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment successful via Stripe"
            };
        }

        public async Task<bool> RefundAsync(string transactionId)
        {
            // refund logic
            return true;
        }
    }
}
