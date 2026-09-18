namespace BackendApi.Configuration
{
    /// <summary>
    /// Hằng số dùng chung cho module User Management.
    /// Tất cả giới hạn và giá trị mặc định được tập trung tại đây — KHÔNG hard-code ở nhiều nơi.
    /// </summary>
    public static class UserManagementConstants
    {
        // ==================== Avatar ====================

        /// <summary>
        /// Dung lượng tối đa avatar: 5MB.
        /// </summary>
        public const long MaxAvatarSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Các extension file avatar được phép.
        /// </summary>
        public static readonly string[] AllowedAvatarExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        /// <summary>
        /// Các content type avatar được phép.
        /// </summary>
        public static readonly string[] AllowedAvatarContentTypes = { "image/jpeg", "image/png", "image/webp" };

        // ==================== Validation ====================

        /// <summary>
        /// Độ dài tối đa FullName.
        /// </summary>
        public const int MaxFullNameLength = 100;

        /// <summary>
        /// Độ dài tối thiểu FullName.
        /// </summary>
        public const int MinFullNameLength = 1;

        /// <summary>
        /// Độ dài tối đa Email.
        /// </summary>
        public const int MaxEmailLength = 255;

        /// <summary>
        /// Độ dài tối đa Phone.
        /// </summary>
        public const int MaxPhoneLength = 20;

        /// <summary>
        /// Độ dài tối thiểu Password.
        /// </summary>
        public const int MinPasswordLength = 6;

        /// <summary>
        /// Độ dài tối đa Password.
        /// </summary>
        public const int MaxPasswordLength = 128;
    }
}
