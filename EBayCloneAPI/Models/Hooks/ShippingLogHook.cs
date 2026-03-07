using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public class ShippingLogHook : IShippingEventHook
    {
        private readonly ApplicationDbContext _db;

        public ShippingLogHook(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnShipmentCreated(OrderTable order, string trackingNumber)
        {
            _db.SystemLogs.Add(new SystemLog
            {
                Module = "Shipping",
                Action = "ShipmentCreated",
                Status = "Success",
                ErrorMessage = null,
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }
}
