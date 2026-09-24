using System.ComponentModel.DataAnnotations;

namespace UM.Core.DTOS.Request.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}