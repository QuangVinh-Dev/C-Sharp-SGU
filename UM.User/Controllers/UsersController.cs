using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackendApi.Services;
using BackendApi.DTOs.Users;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private long GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(idClaim, out var id))
            return id;
        return 0;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var id = GetCurrentUserId();
        if (id == 0) return Unauthorized();

        var profile = await _userService.GetUserProfileAsync(id);
        if (profile == null) return NotFound(new { success = false, message = "User not found" });

        return Ok(new { success = true, data = profile });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var profile = await _userService.GetUserProfileAsync(id);
        if (profile == null) return NotFound(new { success = false, message = "User not found" });
        return Ok(new { success = true, data = profile });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var id = GetCurrentUserId();
        if (id == 0) return Unauthorized();

        var profile = await _userService.UpdateUserProfileAsync(id, request);
        return Ok(new { success = true, data = profile });
    }

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
