using EBayCloneAPI.Models;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public interface IOrderService
    {
        Task<OrderTable> CreateOrderAsync(int userId, int productId, int quantity, string region, string paymentMethod, string authToken, string secureKey, string? couponCode = null);
        Task CancelUnpaidOrdersAsync();
    }
}
