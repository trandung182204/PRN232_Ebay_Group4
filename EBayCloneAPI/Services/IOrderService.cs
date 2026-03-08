using EBayCloneAPI.Models;
using System.Threading.Tasks;
using EBayAPI.Enums;

namespace EBayCloneAPI.Services
{
    public interface IOrderService
    {
        // Create order (PendingPayment)
        Task<OrderTable> CreateOrderAsync(
    int userId,
    int productId,
    int quantity,
    string? addressText,
    string? paymentMethod,
    string? region
);

        // User thanh toán
        Task<bool> PayOrderAsync(
            int orderId,
            string paymentMethod,
            string authToken,
            string secureKey);

        // Auto cancel unpaid
        Task AutoCancelOnlinePayments();

        // Order detail
        Task<OrderTable?> GetOrderDetailAsync(int id);

        // Admin list orders
        Task<object> GetOrdersAsync(int page, int pageSize, OrderStatus? status);

        // Admin update status
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}
