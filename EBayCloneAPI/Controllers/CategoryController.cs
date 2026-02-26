using EBayCloneAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EBayCloneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db) => _db = db;

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var items = await _db.Categories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();
            return Ok(items);
        }
    }
}
