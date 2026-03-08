using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IShippingEventHook
    {
        Task OnShipmentCreated(int orderId, string trackingCode);
    }
}
