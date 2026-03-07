using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IPaymentEventHook
    {
        Task OnPaymentSuccess(OrderTable order);
        Task OnPaymentFailed(OrderTable order);
    }
}
