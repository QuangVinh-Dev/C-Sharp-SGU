namespace BackendApi.Exceptions
{
    /// <summary>
    /// Exception khi mật khẩu không hợp lệ (sai mật khẩu hiện tại, không đủ điều kiện...).
    /// Không chứa plaintext password hoặc hash trong message.
    /// </summary>
    public class InvalidPasswordException : UserManagementException
    {
        public InvalidPasswordException(string message)
            : base(message)
        {
        }
    }
}
