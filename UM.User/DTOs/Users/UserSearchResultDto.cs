using BackendApi.Models;

namespace BackendApi.DTOs
{
    /// <summary>
    /// DTO trả về kết quả tìm kiếm User — chỉ chứa thông tin an toàn.
    /// Không trả về Password, PasswordHash hay thông tin bảo mật.
    /// </summary>
    public class UserSearchResultDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Mapping an toàn từ User model sang SearchResultDto.
        /// Chỉ lấy thông tin công khai, không lấy Email/Phone/PasswordHash.
        /// </summary>
        public static UserSearchResultDto FromUser(User user)
        {
            return new UserSearchResultDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Avatar = user.Avatar,
                Status = user.Status
            };
        }
    }
}
