using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UM.Core.Config; 

namespace UM.Core.Controllers
{
    [ApiController] // Bắt buộc phải có để báo đây là API
    [Route("api/[controller]")] // Đường dẫn sẽ thành /api/home
    [EnableRateLimiting(RateLimitPolicies.Public)] // Gắn Rate Limit
    public class HomeController : ControllerBase // Đổi thành ControllerBase, không phải Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Trả về JSON thay vì View()
            return Ok(new { message = "Chào mừng đến với API UM.Core!" });
        }
    }
}