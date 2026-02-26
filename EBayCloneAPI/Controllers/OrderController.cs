using EBayCloneAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    }
}
