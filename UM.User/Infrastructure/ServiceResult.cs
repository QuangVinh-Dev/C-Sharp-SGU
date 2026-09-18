namespace BackendApi.Infrastructure
{
    /// <summary>
    /// Kết quả trả về chuẩn hóa từ Service layer.
    /// Giúp frontend/caller phân biệt thành công/thất bại mà không cần bắt exception.
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Kết quả có thành công không.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Dữ liệu trả về (null nếu thất bại).
        /// </summary>
        public T? Data { get; private set; }

        /// <summary>
        /// Thông điệp mô tả kết quả hoặc lỗi.
        /// </summary>
        public string Message { get; private set; } = string.Empty;

        /// <summary>
        /// Mã lỗi (dùng để frontend xử lý theo từng loại lỗi).
        /// </summary>
        public string? ErrorCode { get; private set; }

        private ServiceResult() { }

        /// <summary>
        /// Tạo kết quả thành công với dữ liệu.
        /// </summary>
        public static ServiceResult<T> SuccessResult(T data, string message = "")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Tạo kết quả thất bại với thông báo lỗi.
        /// </summary>
        public static ServiceResult<T> FailureResult(string message, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
