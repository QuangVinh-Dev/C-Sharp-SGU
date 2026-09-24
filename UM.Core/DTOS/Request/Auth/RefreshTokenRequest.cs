using System.ComponentModel.DataAnnotations;

namespace UM.Core.DTOS.Request.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
