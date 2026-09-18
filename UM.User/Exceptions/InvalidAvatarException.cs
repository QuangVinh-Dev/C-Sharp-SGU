namespace BackendApi.Exceptions
{
    /// <summary>
    /// Exception khi file avatar không hợp lệ (quá lớn, extension sai, rỗng...).
    /// </summary>
    public class InvalidAvatarException : UserManagementException
    {
        public InvalidAvatarException(string message)
            : base(message)
        {
        }
    }
}
