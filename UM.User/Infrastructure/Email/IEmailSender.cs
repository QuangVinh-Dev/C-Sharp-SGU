using System.Threading.Tasks;

namespace BackendApi.Infrastructure.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlMessage);
}
