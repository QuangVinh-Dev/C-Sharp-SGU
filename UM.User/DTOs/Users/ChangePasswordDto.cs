namespace BackendApi.DTOs
{
    /// <summary>
    /// DTO cho chức năng đổi mật khẩu.
    /// Backend phải kiểm tra NewPassword == ConfirmPassword.
    /// Không bao giờ lưu plaintext password vào model User.
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// Mật khẩu hiện tại — dùng để xác minh trước khi đổi.
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu mới.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Xác nhận mật khẩu mới — phải khớp với NewPassword.
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
