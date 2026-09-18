using BackendApi.Configuration;
using BackendApi.DTOs;
using BackendApi.Exceptions;

namespace BackendApi.Validators
{
    /// <summary>
    /// Validator cho file avatar upload.
    /// Kiểm tra: null, empty, size <= 5MB, extension hợp lệ, content type hợp lệ.
    /// </summary>
    public static class AvatarValidator
    {
        /// <summary>
        /// Validate dữ liệu AvatarUploadDto.
        /// Throw InvalidAvatarException nếu không hợp lệ.
        /// </summary>
        public static void Validate(AvatarUploadDto avatar)
        {
            if (avatar == null)
                throw new InvalidAvatarException("Avatar data cannot be null.");

            // Kiểm tra file tồn tại và không rỗng
            if (string.IsNullOrWhiteSpace(avatar.FileName))
                throw new InvalidAvatarException("Avatar file name is required.");

            if (avatar.DataStream == null || avatar.DataStream == Stream.Null)
                throw new InvalidAvatarException("Avatar file data is empty.");

            if (avatar.FileSize <= 0)
                throw new InvalidAvatarException("Avatar file is empty.");

            // Kiểm tra dung lượng <= 5MB
            if (avatar.FileSize > UserManagementConstants.MaxAvatarSizeBytes)
            {
                var maxSizeMB = UserManagementConstants.MaxAvatarSizeBytes / (1024 * 1024);
                throw new InvalidAvatarException(
                    $"Avatar file size must not exceed {maxSizeMB}MB. Current size: {avatar.FileSize / (1024 * 1024)}MB.");
            }

            // Kiểm tra extension hợp lệ
            var extension = Path.GetExtension(avatar.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                throw new InvalidAvatarException("Avatar file must have a valid extension.");

            if (!UserManagementConstants.AllowedAvatarExtensions.Contains(extension))
            {
                var allowed = string.Join(", ", UserManagementConstants.AllowedAvatarExtensions);
                throw new InvalidAvatarException(
                    $"Avatar file extension '{extension}' is not allowed. Allowed: {allowed}.");
            }

            // Kiểm tra content type nếu được cung cấp
            if (!string.IsNullOrWhiteSpace(avatar.ContentType))
            {
                var contentType = avatar.ContentType.ToLowerInvariant();
                if (!UserManagementConstants.AllowedAvatarContentTypes.Contains(contentType))
                {
                    var allowed = string.Join(", ", UserManagementConstants.AllowedAvatarContentTypes);
                    throw new InvalidAvatarException(
                        $"Avatar content type '{avatar.ContentType}' is not allowed. Allowed: {allowed}.");
                }
            }
        }
    }
}
