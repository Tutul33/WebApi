using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApi.Middlewares;

namespace WebApi.Controllers
{
    [AllowAnonymous]  // No authentication required for public access
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        IConfiguration _configuration = null;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("info")]
        public IActionResult GetPublicInfo()
        {
            return Ok(new { message = "This is public information accessible to everyone." });
        }

        [HttpGet("generateToken")]
        public IActionResult GenerateToken(string userName)
        {
            TokenService tokenService = new TokenService(_configuration);
            var key=tokenService.GenerateToken(userName);
            return Ok(new { message = key });
        }
    }
}
