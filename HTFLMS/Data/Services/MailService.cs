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
        private readonly MailSettings settings;

        public MailService(
            IOptions<MailSettings> mailSettings)
        {
            settings = mailSettings.Value;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return;

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    settings.SenderName,
                    settings.SenderEmail));

            email.To.Add(
                MailboxAddress.Parse(toEmail));

            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,

                TextBody =
                    "Please view this email in an HTML-compatible email client."
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            if (settings.Port == 465)
            {
                await smtp.ConnectAsync(
                    settings.Server,
                    settings.Port,
                    SecureSocketOptions.SslOnConnect);
            }
            else
            {
                await smtp.ConnectAsync(
                    settings.Server,
                    settings.Port,
                    SecureSocketOptions.StartTls);
            }

            await smtp.AuthenticateAsync(
                settings.Username,
                settings.Password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }


        public async Task SendRegistrationEmailAsync(
            string toEmail,
            string name,
            string userId)
        {
            var (subject, body) =
                EmailTemplates.RegistrationEmail(
                    name,
                    userId
                );

            await SendAsync(
                toEmail,
                subject,
                body
            );
        }


        public async Task SendEnrollmentEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle)
        {
            var (subject, body) =
                NotificationEmailTemplates.EnrollmentEmail(
                    studentName,
                    courseTitle);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendNewAssignmentEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate)
        {
            var (subject, body) =
                NotificationEmailTemplates.NewAssignmentEmail(
                    studentName,
                    courseTitle,
                    assignmentTitle,
                    dueDate);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendAssignmentDueReminderEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate)
        {
            var (subject, body) =
                NotificationEmailTemplates.AssignmentDueReminderEmail(
                    studentName,
                    courseTitle,
                    assignmentTitle,
                    dueDate);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendAssignmentGradedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            string? gradeText)
        {
            var (subject, body) =
                NotificationEmailTemplates.AssignmentGradedEmail(
                    studentName,
                    courseTitle,
                    assignmentTitle,
                    gradeText);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCertificateRequestEmailAsync(
            string toEmail,
            string recipientName,
            string studentName,
            string courseTitle,
            bool isReapplication = false)
        {
            var (subject, body) =
                NotificationEmailTemplates.CertificateRequestEmail(
                    recipientName,
                    studentName,
                    courseTitle,
                    isReapplication);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCertificateApprovedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle)
        {
            var (subject, body) =
                NotificationEmailTemplates.CertificateApprovedEmail(
                    studentName,
                    courseTitle);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCertificateRejectedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle)
        {
            var (subject, body) =
                NotificationEmailTemplates.CertificateRejectedEmail(
                    studentName,
                    courseTitle);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCertificateReadyEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string? certificateId)
        {
            var (subject, body) =
                NotificationEmailTemplates.CertificateReadyEmail(
                    studentName,
                    courseTitle,
                    certificateId);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCourseAvailableEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle)
        {
            var (subject, body) =
                NotificationEmailTemplates.CourseAvailableEmail(
                    studentName,
                    courseTitle);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCourseStartEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            DateTime? startDate)
        {
            var (subject, body) =
                NotificationEmailTemplates.CourseStartEmail(
                    studentName,
                    courseTitle,
                    startDate);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCourseEndingEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            DateTime? endDate)
        {
            var (subject, body) =
                NotificationEmailTemplates.CourseEndingEmail(
                    studentName,
                    courseTitle,
                    endDate);

            await SendAsync(
                toEmail,
                subject,
                body);
        }

        public async Task SendCourseCompletedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle)
        {
            var (subject, body) =
                NotificationEmailTemplates.CourseCompletedEmail(
                    studentName,
                    courseTitle);

            await SendAsync(
                toEmail,
                subject,
                body);
        }
    }
}



//using HTFLMS.Data.IServices;
//using HTFLMS.Helper;
//using MailKit.Net.Smtp;
//using MailKit.Security;
//using Microsoft.Extensions.Options;
//using MimeKit;
//namespace HTFLMS.Data.Services
//{
//    public class MailService : IMailService
//    {
//        private readonly MailSettings _s;

//        public MailService(IOptions<MailSettings> settings)
//        {
//            _s = settings.Value;
//        }

//        public async Task SendAsync(string toEmail, string subject, string htmlBody)
//        {
//            var email = new MimeMessage();
//            email.From.Add(new MailboxAddress(_s.SenderName, _s.SenderEmail));
//            email.To.Add(MailboxAddress.Parse(toEmail));
//            email.Subject = subject;

//            var builder = new BodyBuilder
//            {
//                HtmlBody = htmlBody,
//                TextBody = "Please view this email in an HTML-compatible client."
//            };
//            email.Body = builder.ToMessageBody();

//            using var smtp = new SmtpClient();

//            if (_s.Port == 465)
//                await smtp.ConnectAsync(_s.Server, _s.Port, SecureSocketOptions.SslOnConnect);
//            else
//                await smtp.ConnectAsync(_s.Server, _s.Port, SecureSocketOptions.StartTls);

//            await smtp.AuthenticateAsync(_s.Username, _s.Password);
//            await smtp.SendAsync(email);
//            await smtp.DisconnectAsync(true);
//        }
//        public async Task SendRegistrationEmailAsync(
//            string toEmail, string name, string userId, string password)
//        {
//            var (subject, body) = EmailTemplates.RegistrationEmail(name, userId, password);
//            await SendAsync(toEmail, subject, body);
//        }
//    }
//}
