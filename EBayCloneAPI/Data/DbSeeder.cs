using EBayCloneAPI.Data;
using EBayCloneAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // Check if admin exists
            if (!db.Users.Any(u => u.Email == "admin123@ebayclone.com"))
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "admin123@ebayclone.com",
                    Password = "admin123", 
                    Role = "Admin", 
                    AvatarUrl = null             
                };

                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }
        }
    }
}