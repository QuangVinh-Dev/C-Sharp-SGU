using BackendApi.Configuration;
using Microsoft.Extensions.Options;

namespace BackendApi.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;

    public SmtpEmailSender(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("           EMAIL OTP - DEV MODE");
        Console.WriteLine("========================================");
        Console.WriteLine($"To      : {to}");
        Console.WriteLine($"Subject : {subject}");
        Console.WriteLine($"Content : {htmlMessage}");
        Console.WriteLine("========================================");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}