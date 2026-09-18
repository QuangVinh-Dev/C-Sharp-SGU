using System.ComponentModel.DataAnnotations;

namespace BackendApi.DTOs.Users;

public class UserProfileDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PublicCode { get; set; } = string.Empty; // Just mocked logic or use Id if no PublicCode in db
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}
