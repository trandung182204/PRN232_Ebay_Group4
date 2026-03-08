using EBayAPI.DTOs;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class PaymentEmailHook : IPaymentEventHook
    {
        public Task OnPaymentSuccess(OrderTable order)
        {
            if (order == null)
            {
                Console.WriteLine("[HOOK] OnPaymentSuccess called with null order");
                return Task.CompletedTask;
            }

            Console.WriteLine($"[HOOK] Send email for order {order.Id}");
            return Task.CompletedTask;
        }

        public Task OnPaymentFailed(OrderTable order)
        {
            if (order == null)
            {
                Console.WriteLine("[HOOK] OnPaymentFailed called with null order");
                return Task.CompletedTask;
            }

            // no-op for now
            return Task.CompletedTask;
        }
    }
}
