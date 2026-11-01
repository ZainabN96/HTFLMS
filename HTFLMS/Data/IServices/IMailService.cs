namespace HTFLMS.Data.IServices
{
    public interface IMailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody);

        Task SendRegistrationEmailAsync(
            string toEmail,
            string name,
            string userId);
            //string password);

        Task SendEnrollmentEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle);

        Task SendNewAssignmentEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate);

        Task SendAssignmentDueReminderEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate);

        Task SendAssignmentGradedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            string? gradeText);

        Task SendCertificateRequestEmailAsync(
            string toEmail,
            string recipientName,
            string studentName,
            string courseTitle,
            bool isReapplication = false);

        Task SendCertificateApprovedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle);

        Task SendCertificateRejectedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle);

        Task SendCertificateReadyEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            string? certificateId);

        Task SendCourseAvailableEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle);

        Task SendCourseStartEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            DateTime? startDate);

        Task SendCourseEndingEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle,
            DateTime? endDate);

        Task SendCourseCompletedEmailAsync(
            string toEmail,
            string studentName,
            string courseTitle);
    }
}




//namespace HTFLMS.Data.IServices
//{
//    public interface IMailService
//    {
//        Task SendAsync(string toEmail, string subject, string htmlBody);

//        Task SendRegistrationEmailAsync(
//            string toEmail,
//            string name,
//            string userId,
//            string password
//        );
//    }
//}


