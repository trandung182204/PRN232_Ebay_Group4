using EBayCloneAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EBayAPI.Enums;
namespace EBayCloneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrderController(IOrderService orders) => _orders = orders;

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] int productId, [FromForm] int quantity, [FromForm] string region, [FromForm] string paymentMethod, [FromForm] string authToken, [FromForm] string secureKey, [FromForm] int? userId)
        {
            var sessionUser = HttpContext.Session.GetInt32("UserId");
            var uid = sessionUser ?? userId;
            if (!uid.HasValue) return BadRequest("Login required");

            var order = await _orders.CreateOrderAsync(uid.Value, productId, quantity, region, paymentMethod, authToken, secureKey);
            // return a lightweight DTO to avoid JSON cycles from EF navigation properties
            return Ok(new { id = order.Id, status = order.Status, total = order.TotalPrice });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _orders.GetOrderDetailAsync(id);

            if (order == null)
                return NotFound();

            var result = new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                Status = order.Status.ToString(),

                OrderItems = order.OrderItems.Select(i => new
                {
                    i.Quantity,
                    Product = new
                    {
                        i.Product.Title,
                        i.Product.Price
                    }
                }),

                Payments = order.Payments.Select(p => new
                {
                    p.Method,
                    p.Amount,
                    p.PaidAt
                }),

                ShippingInfos = order.ShippingInfos.Select(s => new
                {
                    s.TrackingNumber,
                    s.Status
                })
            };

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders(int page = 1, int pageSize = 10, OrderStatus? status = null)
        {
            var result = await _orders.GetOrdersAsync(page, pageSize, status);

            return Ok(result);
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
        {
            var updated = await _orders.UpdateOrderStatusAsync(id, status);

            if (!updated)
                return NotFound();

            return Ok(new { message = "Status updated" });
        }
    }

}
