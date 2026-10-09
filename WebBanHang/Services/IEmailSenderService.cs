using System.Threading.Tasks;

namespace WebBanHang.Services
{
    public interface IEmailSenderService
    {
        Task<(bool Success, string Message)> SendEmailAsync(string toEmail, string subject, string body);
    }
}
