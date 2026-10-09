using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebBanHang.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPortStr = _configuration["Smtp:Port"];
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "no-reply@eco2415.vn";

            // Nếu chưa cấu hình SMTP thật, hệ thống hoạt động ở chế độ Giả lập / Mocking an toàn
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogInformation("Simulated Email sent to {ToEmail}. Subject: {Subject}", toEmail, subject);
                await Task.Delay(500); // Giả lập độ trễ mạng
                return (true, $"Đã gửi thư nhắc nợ thành công tới khách hàng ({toEmail}) qua hệ thống giả lập.");
            }

            try
            {
                int port = int.TryParse(smtpPortStr, out var p) ? p : 587;
                using var client = new SmtpClient(smtpHost, port)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Phòng Kế Toán ECO2415"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                return (true, $"Đã gửi email nhắc nợ thành công tới {toEmail} qua máy chủ SMTP {smtpHost}!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email thực tế qua SMTP.");
                return (false, $"Lỗi kết nối máy chủ gửi mail: {ex.Message}");
            }
        }
    }
}
