using EBayCloneAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EBayCloneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _products;
        public ProductController(IProductRepository products) => _products = products;

        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int? categoryId, [FromQuery] string? orderBy, [FromQuery] string? sortDir, [FromQuery] int page = 1, [FromQuery] int pageSize = 16)
        {
            var (items, total) = await _products.ListAsync(q, categoryId, orderBy, sortDir, page, pageSize);
            var dto = items.Select(p => new {
                id = p.Id,
                title = p.Title,
                description = p.Description,
                price = p.Price,
                images = p.Images,
                seller = p.Seller == null ? null : new { id = p.Seller.Id, email = p.Seller.Email, username = p.Seller.Username },
                reviews = p.Reviews == null ? null : p.Reviews.Select(r => new { id = r.Id, rating = r.Rating, comment = r.Comment })
            });
            return Ok(new { items = dto, total });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _products.GetByIdAsync(id);
            if (p == null) return NotFound();
            var dto = new {
                id = p.Id,
                title = p.Title,
                description = p.Description,
                price = p.Price,
                images = p.Images,
                seller = p.Seller == null ? null : new { id = p.Seller.Id, email = p.Seller.Email, username = p.Seller.Username },
                reviews = p.Reviews == null ? null : p.Reviews.Select(r => new { id = r.Id, rating = r.Rating, comment = r.Comment })
            };
            return Ok(dto);
        }
    }
}
