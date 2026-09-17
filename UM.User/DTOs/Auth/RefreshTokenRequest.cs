using System.ComponentModel.DataAnnotations;

namespace BackendApi.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
