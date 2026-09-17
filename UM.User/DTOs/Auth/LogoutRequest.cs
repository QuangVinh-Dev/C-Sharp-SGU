using System.ComponentModel.DataAnnotations;

namespace BackendApi.DTOs.Auth;

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
