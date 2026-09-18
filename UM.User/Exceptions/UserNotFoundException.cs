namespace BackendApi.Exceptions
{
    /// <summary>
    /// Exception khi không tìm thấy User với UserId được cung cấp.
    /// Không chứa thông tin nhạy cảm — chỉ báo UserId không tồn tại.
    /// </summary>
    public class UserNotFoundException : UserManagementException
    {
        public int UserId { get; }

        public UserNotFoundException(int userId)
            : base($"User with ID {userId} was not found.")
        {
            UserId = userId;
        }
    }
}
