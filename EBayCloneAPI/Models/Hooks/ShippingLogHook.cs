using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class ShippingLogHook : IShippingEventHook
    {
        public Task OnShipmentCreated(int orderId, string trackingCode)
        {
            Console.WriteLine($"[HOOK] Shipment created: {trackingCode} for order {orderId}");
            return Task.CompletedTask;
        }
    }
}
