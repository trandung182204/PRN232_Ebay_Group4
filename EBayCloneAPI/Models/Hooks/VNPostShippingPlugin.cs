using EBayAPI.DTOs;

namespace EBayAPI.Models.Hooks
{
    public class VNPostShippingPlugin : IShippingHook
    {
        public string Name => "VNPost";

        public async Task<ShippingQuote> GetQuoteAsync(ShippingRequest request)
        {
            return new ShippingQuote
            {
                Provider = "VNPost",
                Price = 25000
            };
        }

        public async Task<string> CreateShipmentAsync(ShippingRequest request)
        {
            return "VNPOST_TRACKING_123";
        }
    }
}
