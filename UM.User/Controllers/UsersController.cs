using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetMe() => Ok(new { success = true, data = "Get current user" });

    [HttpGet("{id}")]
    public IActionResult GetUser(long id) => Ok(new { success = true, data = "Get user by ID" });

    [HttpPut("me")]
    public IActionResult UpdateMe() => Ok(new { success = true, data = "Update current user" });

    [HttpGet]
    [Authorize(Policy = "users.read")]
    public IActionResult GetUsers() => Ok(new { success = true, data = "List users" });

    [HttpPatch("{id}/suspend")]
    [Authorize(Policy = "users.suspend")]
    public IActionResult SuspendUser(long id) => Ok(new { success = true, data = "Suspend user" });

    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "users.update")]
    public IActionResult ActivateUser(long id) => Ok(new { success = true, data = "Activate user" });
}
