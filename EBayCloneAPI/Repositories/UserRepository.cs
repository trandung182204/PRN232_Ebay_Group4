using EBayCloneAPI.Data;
using EBayCloneAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EBayCloneAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) => _db = db;

        public async Task<User?> GetByEmailAsync(string email) => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdAsync(int id) => await _db.Users.FindAsync(id);
    }
}
