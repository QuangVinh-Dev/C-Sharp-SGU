using System.Net;
using System.Net.Mail;

namespace UM.Core.Service;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp, string purpose, int expirationMinutes)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var host = emailSettings["Host"]!;
        var port = int.Parse(emailSettings["Port"]!);
        var username = emailSettings["Username"]!;
        var password = emailSettings["Password"]!;
        var fromEmail = emailSettings["FromEmail"]!;
        var fromName = emailSettings["FromName"] ?? "UM System";

        var subject = $"[UM] Mã OTP - {purpose}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Mã xác thực OTP</h2>
                <p>Mục đích: <strong>{purpose}</strong></p>
                <p>Mã OTP của bạn là:</p>
                <div style='font-size: 32px; font-weight: bold; color: #2563eb;
                            padding: 16px; background: #f0f4ff; border-radius: 8px;
                            display: inline-block; letter-spacing: 8px;'>
                    {otp}
                </div>
                <p>Mã này sẽ hết hạn sau <strong>{expirationMinutes} phút</strong>.</p>
                <hr />
                <p style='color: #dc2626; font-weight: bold;'>
                    ⚠️ KHÔNG chia sẻ mã OTP này với bất kỳ ai.
                    Nhân viên hỗ trợ sẽ không bao giờ yêu cầu mã OTP của bạn.
                </p>
            </body>
            </html>";

        using var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("OTP email sent to {Email} for purpose {Purpose}", toEmail, purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
            throw;
        }
    }
}
