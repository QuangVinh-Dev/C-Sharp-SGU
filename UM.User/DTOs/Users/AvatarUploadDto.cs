namespace BackendApi.DTOs
{
    /// <summary>
    /// Abstraction cho dữ liệu file upload avatar.
    /// Không phụ thuộc vào IFormFile (ASP.NET) hay StorageFile (WinUI).
    /// Sau này frontend/web layer sẽ convert từ framework cụ thể sang DTO này.
    /// </summary>
    public class AvatarUploadDto
    {
        /// <summary>
        /// Tên file gốc (ví dụ: "photo.jpg").
        /// Không được dùng trực tiếp làm đường dẫn lưu trữ — backend sẽ tạo tên GUID.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Content type của file (ví dụ: "image/jpeg").
        /// Dùng để validate — có thể null nếu frontend không cung cấp.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Dung lượng file tính bằng bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Stream dữ liệu file.
        /// </summary>
        public Stream DataStream { get; set; } = Stream.Null;
    }
}
