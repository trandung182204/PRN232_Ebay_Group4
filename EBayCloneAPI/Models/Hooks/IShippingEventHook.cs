using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IShippingEventHook
    {
        Task OnShipmentCreatedAsync(OrderTable order, string trackingNumber);
    }
}
