namespace UM.Core.Service;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otp, string purpose, int expirationMinutes);
}
