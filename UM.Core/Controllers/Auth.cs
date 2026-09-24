using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UM.Core.DTOS.Request.Auth;
using UM.Core.Service;

namespace UM.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterAsync(request, ipAddress);
            return StatusCode(201, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/verify-otp
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.VerifyOtpAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/refresh
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(request, ipAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/logout
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = GetIpAddress();
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")
                                ?? "0");

        await _authService.LogoutAsync(request.RefreshToken, ipAddress, userId);
        return Ok(new { message = "Đăng xuất thành công." });
    }

    /// <summary>
    /// POST /api/auth/forgot-password
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var ipAddress = GetIpAddress();
        var result = await _authService.ForgotPasswordAsync(request, ipAddress);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/auth/reset-password
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.ResetPasswordAsync(request, ipAddress);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Helper ───────────────────────────────────────────────────

    private string? GetIpAddress()
    {
        // Check for forwarded IP first (reverse proxy scenarios)
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault();
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
