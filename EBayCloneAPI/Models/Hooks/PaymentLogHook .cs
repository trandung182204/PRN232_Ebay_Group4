using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class PaymentLogHook : IPaymentEventHook
    {
        public Task OnPaymentSuccess(OrderTable order)
        {
            Console.WriteLine($"[HOOK] Payment success for order {order.Id}");
            return Task.CompletedTask;
        }

        public Task OnPaymentFailed(OrderTable order)
        {
            Console.WriteLine($"[HOOK] Payment failed for order {order.Id}");
            return Task.CompletedTask;
        }
    }
}
