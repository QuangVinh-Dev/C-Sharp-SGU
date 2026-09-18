namespace BackendApi.Infrastructure
{
    /// <summary>
    /// Abstraction cho file storage (lưu/xóa/lấy đường dẫn avatar).
    /// CHƯA implement lưu file thật.
    /// 
    /// Sau này có thể implement:
    ///   LocalFileStorage : IFileStorage      (lưu local disk)
    ///   CloudFileStorage : IFileStorage      (lưu cloud: Azure Blob, AWS S3...)
    /// 
    /// UserService không cần biết file thực sự được lưu ở đâu.
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Lưu file avatar và trả về đường dẫn/key đã lưu.
        /// </summary>
        Task<string> SaveAvatarAsync(string fileName, Stream dataStream);

        /// <summary>
        /// Xóa file avatar theo đường dẫn.
        /// </summary>
        Task<bool> DeleteAvatarAsync(string filePath);

        /// <summary>
        /// Lấy URL/đường dẫn truy cập avatar.
        /// </summary>
        string GetAvatarUrl(string filePath);
    }
}
