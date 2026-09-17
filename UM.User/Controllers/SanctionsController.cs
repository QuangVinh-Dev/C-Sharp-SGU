using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "sanctions.read")]
public class SanctionsController : ControllerBase
{
    [HttpGet("users")]
    public IActionResult GetUserSanctions() => Ok(new { success = true, data = "List user sanctions" });

    [HttpPost("users")]
    [Authorize(Policy = "sanctions.create")]
    public IActionResult CreateUserSanction() => Ok(new { success = true, data = "Create user sanction" });

    [HttpPost("users/{id}/revoke")]
    [Authorize(Policy = "sanctions.revoke")]
    public IActionResult RevokeUserSanction(long id) => Ok(new { success = true, data = "Revoke user sanction" });
}
