namespace BackendApi.Security
{
    /// <summary>
    /// Abstraction cho password hashing.
    /// CHƯA implement thuật toán cụ thể — chờ Authentication team thống nhất.
    /// 
    /// Sau này có thể implement:
    ///   BCryptPasswordHasher : IPasswordHasher
    ///   Argon2PasswordHasher : IPasswordHasher
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hash mật khẩu plaintext thành chuỗi hash.
        /// </summary>
        string Hash(string password);

        /// <summary>
        /// Xác minh mật khẩu plaintext có khớp với hash đã lưu không.
        /// </summary>
        bool Verify(string password, string passwordHash);
    }
}
