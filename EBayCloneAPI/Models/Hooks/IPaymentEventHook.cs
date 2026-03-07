using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IPaymentEventHook
    {
        Task OnPaymentSuccessAsync(OrderTable order, Payment payment, string transactionId);
    }
}
