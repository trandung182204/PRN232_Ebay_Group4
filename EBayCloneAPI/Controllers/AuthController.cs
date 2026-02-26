using EBayCloneAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EBayCloneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _users;

        public AuthController(IUserRepository users) => _users = users;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var user = await _users.GetByEmailAsync(email);
            if (user == null) return BadRequest("Invalid credentials");
            if (user.Password != password) return BadRequest("Invalid credentials");

            HttpContext.Session.SetInt32("UserId", user.Id);
            return Ok(new { user.Id, user.Email, user.Username });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return Ok();
        }
    }
}
