namespace BackendApi.DTOs
{
    /// <summary>
    /// DTO chứa dữ liệu user được phép cập nhật.
    /// Không cho phép thay đổi: UserId, Username, PasswordHash, Role, Permission, CreatedDate.
    /// Client KHÔNG được gửi trực tiếp User object để update.
    /// </summary>
    public class UpdateProfileDto
    {
        /// <summary>
        /// Tên đầy đủ — bắt buộc, giới hạn theo UserManagementConstants.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email — phải đúng format.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại — tùy chọn.
        /// </summary>
        public string? Phone { get; set; }
    }
}
