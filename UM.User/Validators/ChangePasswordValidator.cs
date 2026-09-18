using BackendApi.Configuration;
using BackendApi.DTOs;
using BackendApi.Exceptions;

namespace BackendApi.Validators
{
    /// <summary>
    /// Validator cho dữ liệu đổi mật khẩu (ChangePasswordDto).
    /// Kiểm tra tại backend — không phụ thuộc frontend validation.
    /// </summary>
    public static class ChangePasswordValidator
    {
        /// <summary>
        /// Validate dữ liệu ChangePasswordDto.
        /// Throw InvalidPasswordException nếu không hợp lệ.
        /// </summary>
        public static void Validate(ChangePasswordDto dto)
        {
            if (dto == null)
                throw new InvalidPasswordException("Change password data cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new InvalidPasswordException("Current password is required.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new InvalidPasswordException("New password is required.");

            if (dto.NewPassword.Length < UserManagementConstants.MinPasswordLength)
                throw new InvalidPasswordException(
                    $"New password must be at least {UserManagementConstants.MinPasswordLength} characters.");

            if (dto.NewPassword.Length > UserManagementConstants.MaxPasswordLength)
                throw new InvalidPasswordException(
                    $"New password must not exceed {UserManagementConstants.MaxPasswordLength} characters.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                throw new InvalidPasswordException("Confirm password is required.");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new InvalidPasswordException("New password and confirm password do not match.");

            if (dto.CurrentPassword == dto.NewPassword)
                throw new InvalidPasswordException("New password must be different from current password.");
        }
    }
}
