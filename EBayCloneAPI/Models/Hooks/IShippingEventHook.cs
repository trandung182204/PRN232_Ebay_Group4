using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IShippingEventHook
    {
        Task OnShipmentCreated(OrderTable order, string trackingNumber);
    }
}
