using EBayCloneAPI.Data;
using EBayCloneAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EBayCloneAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Product>> ListAsync(int limit = 50)
        {
            return await _db.Products.AsNoTracking().Include(p => p.Seller).Include(p => p.Reviews).Take(limit).ToListAsync();
        }

        public async Task<(IEnumerable<Product> Items, int Total)> ListAsync(string? q, int? categoryId, string? orderBy, string? sortDir, int page, int pageSize)
        {
            var query = _db.Products.AsNoTracking().Include(p => p.Seller).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Title!.Contains(q) || (p.Description != null && p.Description.Contains(q)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // ordering
            var desc = string.Equals(sortDir, "desc", System.StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(orderBy))
            {
                if (orderBy == "price")
                    query = desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);
                else if (orderBy == "title")
                    query = desc ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title);
                else
                    query = query.OrderByDescending(p => p.Id);
            }
            else
            {
                query = query.OrderByDescending(p => p.Id);
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Product?> GetByIdAsync(int id) => await _db.Products.Include(p => p.Seller).Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == id);
    }
}
