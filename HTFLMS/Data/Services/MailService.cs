using HTFLMS.Data.IServices;
using HTFLMS.Helper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
namespace HTFLMS.Data.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _s;

        public MailService(IOptions<MailSettings> settings)
        {
            _s = settings.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = "Please view this email in an HTML-compatible client."
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            if (_s.Port == 465)
                await smtp.ConnectAsync(_s.Server, _s.Port, SecureSocketOptions.SslOnConnect);
            else
                await smtp.ConnectAsync(_s.Server, _s.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_s.Username, _s.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        public async Task SendRegistrationEmailAsync(
            string toEmail, string name, string userId, string password)
        {
            var (subject, body) = EmailTemplates.RegistrationEmail(name, userId, password);
            await SendAsync(toEmail, subject, body);
        }
    }
}
