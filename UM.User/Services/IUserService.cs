using BackendApi.DTOs;
using BackendApi.Infrastructure;

namespace BackendApi.Services
{
    /// <summary>
    /// Interface định nghĩa các business operations cho User Management.
    /// Đây là contract mà frontend/caller sẽ sử dụng.
    /// Service xử lý BUSINESS LOGIC — không có SQL, không có UI code.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Lấy profile user theo UserId.
        /// Trả về UserProfileDto — KHÔNG chứa PasswordHash.
        /// </summary>
        Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId);

        /// <summary>
        /// Cập nhật profile user.
        /// Chỉ cho phép thay đổi FullName, Email, Phone.
        /// </summary>
        Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);

        /// <summary>
        /// Upload avatar cho user.
        /// Kiểm tra file size <= 5MB, extension hợp lệ.
        /// </summary>
        Task<ServiceResult<string>> UploadAvatarAsync(int userId, AvatarUploadDto avatar);

        /// <summary>
        /// Đổi mật khẩu user.
        /// Xác minh mật khẩu hiện tại trước khi đổi.
        /// </summary>
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        /// <summary>
        /// Tìm kiếm user theo UserId.
        /// Trả về UserSearchResultDto — chỉ thông tin an toàn.
        /// </summary>
        Task<ServiceResult<UserSearchResultDto>> SearchUserByIdAsync(int userId);
    }
}
