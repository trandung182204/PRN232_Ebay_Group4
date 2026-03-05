using EBayAPI.DTOs;
using EBayCloneAPI.Models;

namespace EBayAPI.Models.Hooks
{
    public interface IShippingHook
    {
        string Name { get; }

        Task<ShippingQuote> GetQuoteAsync(ShippingRequest request);

        Task<string> CreateShipmentAsync(ShippingRequest request);
    }
}
