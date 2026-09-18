namespace BackendApi.Exceptions
{
    /// <summary>
    /// Base exception cho tất cả lỗi trong module User Management.
    /// Các exception cụ thể kế thừa từ class này.
    /// </summary>
    public class UserManagementException : Exception
    {
        public UserManagementException(string message)
            : base(message)
        {
        }

        public UserManagementException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
