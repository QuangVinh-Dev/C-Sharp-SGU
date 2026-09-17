using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BackendApi.Configuration;

namespace BackendApi.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;

    public SmtpEmailSender(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_emailOptions.Host)) return; // Or throw

        var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailOptions.From),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}
