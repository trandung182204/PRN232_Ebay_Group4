using EBayCloneAPI.Models;
using System.Threading.Tasks;
using EBayAPI.Enums;
namespace EBayCloneAPI.Services
{
    public interface IOrderService
    {
        Task<OrderTable> CreateOrderAsync(int userId, int productId, int quantity, string region, string paymentMethod, string authToken, string secureKey, string? couponCode = null);
        Task CancelUnpaidOrdersAsync();
        Task<OrderTable?> GetOrderDetailAsync(int id);

        Task<object> GetOrdersAsync(int page, int pageSize, OrderStatus? status);

        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}
