using System.Security.Cryptography;
using BCrypt.Net;

namespace BackendApi.Security;

public interface IOtpService
{
    string GenerateOtp();
    byte[] HashOtp(string otp);
    bool VerifyOtp(string otp, byte[] hash);
}

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    public byte[] HashOtp(string otp)
    {
        return SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(otp));
    }

    public bool VerifyOtp(string otp, byte[] hash)
    {
        var inputHash = HashOtp(otp);
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
