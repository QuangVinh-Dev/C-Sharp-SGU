using BackendApi.Models;

namespace BackendApi.DTOs
{
    /// <summary>
    /// DTO trả về thông tin profile user cho frontend.
    /// TUYỆT ĐỐI KHÔNG chứa Password, PasswordHash, PasswordSalt hay thông tin xác thực nhạy cảm.
    /// </summary>
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Mapping an toàn từ User model sang DTO.
        /// Đảm bảo PasswordHash KHÔNG BAO GIỜ bị leak ra ngoài.
        /// </summary>
        public static UserProfileDto FromUser(User user)
        {
            return new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                CreatedDate = user.CreatedDate,
                Status = user.Status
            };
        }
    }
}
