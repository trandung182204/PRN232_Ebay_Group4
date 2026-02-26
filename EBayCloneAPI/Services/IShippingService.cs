namespace EBayCloneAPI.Services
{
    public interface IShippingService
    {
        Task<(bool success, string trackingCode)> CreateShipmentAsync(int orderId, string region, string authToken, string secureKey);
        Task<(bool success, string status)> GetShipmentStatusAsync(string trackingCode);
    }
}
