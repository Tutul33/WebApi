using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Authorize(Policy = "PrivateAccess")]  // Authentication required for private access
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController()
        {

        }

        [HttpGet("info")]
        public IActionResult GetPrivateInfo()
        {
            return Ok(new { message = "This is private information accessible to authenticated users." });
        }
    }
}
