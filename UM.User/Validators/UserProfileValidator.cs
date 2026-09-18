using System.Text.RegularExpressions;
using BackendApi.Configuration;
using BackendApi.DTOs;
using BackendApi.Exceptions;

namespace BackendApi.Validators
{
    /// <summary>
    /// Validator cho thông tin profile user (UpdateProfileDto).
    /// Validation nằm ở backend — frontend validation chỉ là lớp bổ sung.
    /// </summary>
    public static class UserProfileValidator
    {
        /// <summary>
        /// Validate dữ liệu UpdateProfileDto.
        /// Throw UserManagementException nếu không hợp lệ.
        /// </summary>
        public static void Validate(UpdateProfileDto dto)
        {
            if (dto == null)
                throw new UserManagementException("Profile data cannot be null.");

            ValidateFullName(dto.FullName);
            ValidateEmail(dto.Email);
            ValidatePhone(dto.Phone);
        }

        private static void ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new UserManagementException("Full name is required.");

            if (fullName.Length < UserManagementConstants.MinFullNameLength)
                throw new UserManagementException(
                    $"Full name must be at least {UserManagementConstants.MinFullNameLength} character(s).");

            if (fullName.Length > UserManagementConstants.MaxFullNameLength)
                throw new UserManagementException(
                    $"Full name must not exceed {UserManagementConstants.MaxFullNameLength} characters.");
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new UserManagementException("Email is required.");

            if (email.Length > UserManagementConstants.MaxEmailLength)
                throw new UserManagementException(
                    $"Email must not exceed {UserManagementConstants.MaxEmailLength} characters.");

            // Kiểm tra format email cơ bản
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new UserManagementException("Email format is invalid.");
        }

        private static void ValidatePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return; // Phone là tùy chọn

            if (phone.Length > UserManagementConstants.MaxPhoneLength)
                throw new UserManagementException(
                    $"Phone number must not exceed {UserManagementConstants.MaxPhoneLength} characters.");

            // Kiểm tra phone chỉ chứa số, dấu +, dấu - và khoảng trắng
            if (!Regex.IsMatch(phone, @"^[\d\s\+\-\(\)]+$"))
                throw new UserManagementException("Phone number format is invalid.");
        }
    }
}
