using BackendApi.DTOs;
using BackendApi.Models;

namespace BackendApi.Repositories
{
    /// <summary>
    /// Interface định nghĩa các thao tác truy cập dữ liệu User.
    /// CHỈ LÀ INTERFACE — chưa implement database.
    /// 
    /// Sau này Database team sẽ tạo:
    ///   SqlUserRepository : IUserRepository
    /// mà UserService không cần thay đổi.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Lấy User theo UserId (bao gồm PasswordHash cho xử lý nội bộ).
        /// </summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>
        /// Cập nhật thông tin profile (FullName, Email, Phone).
        /// </summary>
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);

        /// <summary>
        /// Cập nhật PasswordHash sau khi đổi mật khẩu.
        /// </summary>
        Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash);

        /// <summary>
        /// Cập nhật đường dẫn avatar.
        /// </summary>
        Task<bool> UpdateAvatarAsync(int userId, string avatarPath);

        /// <summary>
        /// Kiểm tra User có tồn tại không.
        /// </summary>
        Task<bool> UserExistsAsync(int userId);

        /// <summary>
        /// Tìm kiếm User theo UserId (dùng cho chức năng search).
        /// </summary>
        Task<User?> SearchByIdAsync(int userId);
    }
}
