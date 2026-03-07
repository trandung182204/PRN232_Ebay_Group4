using EBayAPI.DTOs;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class PaymentEmailHook : IPaymentEventHook
    {
        public Task OnPaymentSuccess(OrderTable order)
        {
            Console.WriteLine($"[HOOK] Send email for order {order.Id}");
            return Task.CompletedTask;
        }

        public Task OnPaymentFailed(OrderTable order)
        {
            return Task.CompletedTask;
        }
    }
}
