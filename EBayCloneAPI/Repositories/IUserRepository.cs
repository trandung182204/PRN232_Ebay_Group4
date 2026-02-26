using EBayCloneAPI.Models;
using System.Threading.Tasks;

namespace EBayCloneAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
    }
}
