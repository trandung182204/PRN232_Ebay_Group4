using Microsoft.AspNetCore.Mvc;
using System;

namespace EBayCloneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        [HttpGet("info")]
        public IActionResult GetSystemInfo()
        {
            return Ok(new
            {
                Title = "EBayClone API Load Balancing Demo",
                ServerId = Environment.MachineName,
                Message = $"Request processed by API Container: {Environment.MachineName}"
            });
        }
    }
}
