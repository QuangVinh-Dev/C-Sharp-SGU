using System.ComponentModel.DataAnnotations;

namespace BackendApi.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
