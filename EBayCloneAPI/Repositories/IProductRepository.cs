using EBayCloneAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EBayCloneAPI.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> ListAsync(int limit = 50);
        Task<(IEnumerable<Product> Items, int Total)> ListAsync(string? q, int? categoryId, string? orderBy, string? sortDir, int page, int pageSize);
        Task<Product?> GetByIdAsync(int id);
    }
}
