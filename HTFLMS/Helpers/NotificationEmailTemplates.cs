using System.Net;

namespace HTFLMS.Helper
{
    public static class NotificationEmailTemplates
    {
        public static (string Subject, string Body) EnrollmentEmail(
            string studentName,
            string courseTitle)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var subject = $"Enrollment Confirmed - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your enrollment in
                    <strong>{safeCourse}</strong>
                    has been successfully confirmed.
                </p>

                <p>
                    You can log in to the HTF LMS to access your course
                    and view the available learning content.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Enrollment Confirmed",
                    content));
        }

        public static (string Subject, string Body) NewAssignmentEmail(
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);
            var safeAssignment = Encode(assignmentTitle);

            var dueDateText = FormatDate(dueDate);

            var subject = $"New Assignment - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    A new assignment has been added to your course
                    <strong>{safeCourse}</strong>.
                </p>

                <p>
                    <strong>Assignment:</strong> {safeAssignment}<br />
                    <strong>Due Date:</strong> {dueDateText}
                </p>

                <p>
                    Please log in to the HTF LMS to review the assignment
                    details and submit your work before the deadline.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "New Assignment",
                    content));
        }

        public static (string Subject, string Body) AssignmentDueReminderEmail(
            string studentName,
            string courseTitle,
            string assignmentTitle,
            DateTime? dueDate)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);
            var safeAssignment = Encode(assignmentTitle);

            var dueDateText = FormatDate(dueDate);

            var subject = $"Assignment Due Reminder - {assignmentTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    This is a reminder that your assignment for
                    <strong>{safeCourse}</strong>
                    is approaching its deadline.
                </p>

                <p>
                    <strong>Assignment:</strong> {safeAssignment}<br />
                    <strong>Due Date:</strong> {dueDateText}
                </p>

                <p>
                    Please log in to the HTF LMS and submit your assignment
                    before the due date.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Assignment Due Reminder",
                    content));
        }

        public static (string Subject, string Body) AssignmentGradedEmail(
            string studentName,
            string courseTitle,
            string assignmentTitle,
            string? gradeText)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);
            var safeAssignment = Encode(assignmentTitle);

            var gradeSection = string.IsNullOrWhiteSpace(gradeText)
                ? ""
                : $@"
                    <p>
                        <strong>Grade:</strong> {Encode(gradeText)}
                    </p>";

            var subject = $"Assignment Graded - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your assignment has been graded for
                    <strong>{safeCourse}</strong>.
                </p>

                <p>
                    <strong>Assignment:</strong> {safeAssignment}
                </p>

                {gradeSection}

                <p>
                    Please log in to the HTF LMS to view your grade
                    and any available feedback.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Assignment Graded",
                    content));
        }

        public static (string Subject, string Body) CertificateRequestEmail(
            string recipientName,
            string studentName,
            string courseTitle,
            bool isReapplication)
        {
            var safeRecipient = Encode(recipientName);
            var safeStudent = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var heading = isReapplication
                ? "Certificate Reapplication Received"
                : "New Certificate Request";

            var subject = isReapplication
                ? $"Certificate Reapplication - {courseTitle}"
                : $"New Certificate Request - {courseTitle}";

            var requestText = isReapplication
                ? "has reapplied for a certificate"
                : "has submitted a certificate request";

            var content = $@"
                <p>Dear {safeRecipient},</p>

                <p>
                    <strong>{safeStudent}</strong>
                    {requestText}
                    for <strong>{safeCourse}</strong>.
                </p>

                <p>
                    Please log in to the HTF LMS to review the request
                    and take the required action.
                </p>";

            return (
                subject,
                BuildTemplate(
                    heading,
                    content));
        }

        public static (string Subject, string Body) CertificateApprovedEmail(
            string studentName,
            string courseTitle)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var subject = $"Certificate Approved - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your certificate request for
                    <strong>{safeCourse}</strong>
                    has been approved.
                </p>

                <p>
                    Your certificate will be available through the HTF LMS
                    once the certificate generation process is completed.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Certificate Approved",
                    content));
        }

        public static (string Subject, string Body) CertificateRejectedEmail(
            string studentName,
            string courseTitle)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var subject = $"Certificate Request Update - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your certificate request for
                    <strong>{safeCourse}</strong>
                    has been rejected.
                </p>

                <p>
                    Please log in to the HTF LMS to review your certificate
                    status. You may reapply when the applicable requirements
                    have been completed.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Certificate Request Update",
                    content));
        }

        public static (string Subject, string Body) CertificateReadyEmail(
            string studentName,
            string courseTitle,
            string? certificateId)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var certificateSection =
                string.IsNullOrWhiteSpace(certificateId)
                    ? ""
                    : $@"
                        <p>
                            <strong>Certificate ID:</strong>
                            {Encode(certificateId)}
                        </p>";

            var subject = $"Your Certificate Is Ready - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your certificate for
                    <strong>{safeCourse}</strong>
                    is now available.
                </p>

                {certificateSection}

                <p>
                    Please log in to the HTF LMS to view or download
                    your certificate.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Certificate Ready",
                    content));
        }

        public static (string Subject, string Body) CourseAvailableEmail(
            string studentName,
            string courseTitle)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var subject = $"Course Available - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your course
                    <strong>{safeCourse}</strong>
                    is now available on the HTF LMS.
                </p>

                <p>
                    Please log in to access the course and review the
                    available learning content.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Course Available",
                    content));
        }

        public static (string Subject, string Body) CourseStartEmail(
            string studentName,
            string courseTitle,
            DateTime? startDate)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var dateText = FormatDate(startDate);

            var subject = $"Your Course Starts Today - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your course
                    <strong>{safeCourse}</strong>
                    is starting.
                </p>

                <p>
                    <strong>Start Date:</strong> {dateText}
                </p>

                <p>
                    Please log in to the HTF LMS to access your course
                    content and begin your learning journey.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Course Starting",
                    content));
        }

        public static (string Subject, string Body) CourseEndingEmail(
            string studentName,
            string courseTitle,
            DateTime? endDate)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var dateText = FormatDate(endDate);

            var subject = $"Course Ending Reminder - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your course
                    <strong>{safeCourse}</strong>
                    is approaching its completion date.
                </p>

                <p>
                    <strong>Course End Date:</strong> {dateText}
                </p>

                <p>
                    Please ensure that your pending lessons,
                    assignments and required course activities are completed
                    before the course ends.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Course Ending Reminder",
                    content));
        }

        public static (string Subject, string Body) CourseCompletedEmail(
            string studentName,
            string courseTitle)
        {
            var safeName = Encode(studentName);
            var safeCourse = Encode(courseTitle);

            var subject = $"Course Completed - {courseTitle}";

            var content = $@"
                <p>Dear {safeName},</p>

                <p>
                    Your course
                    <strong>{safeCourse}</strong>
                    has reached its completion date.
                </p>

                <p>
                    Please log in to the HTF LMS to review your course status,
                    grades and certificate eligibility.
                </p>";

            return (
                subject,
                BuildTemplate(
                    "Course Completed",
                    content));
        }

        private static string BuildTemplate(
            string heading,
            string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>

<body style=""margin:0; padding:0; background-color:#fC9828F;
             font-family:Arial, Helvetica, sans-serif;"">

    <table width=""100%""
           cellpadding=""0""
           cellspacing=""0""
           border=""0""
           style=""background-color:#f5f5f5; padding:30px 15px;"">

        <tr>
            <td align=""center"">

                <table width=""600""
                       cellpadding=""0""
                       cellspacing=""0""
                       border=""0""
                       style=""max-width:600px;
                              background-color:#ffffff;
                              border-radius:8px;
                              overflow:hidden;"">

                    <tr>
                        <td style=""background-color:#C9828F;
                                   padding:22px 30px;
                                   text-align:center;"">

                            <div style=""font-size:22px;
                                        font-weight:bold;
                                        color:#ffffff;"">
                                HCC Technology Foundation
                            </div>

                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:30px;"">

                            <h2 style=""margin-top:0;
                                       color:#222222;
                                       font-size:22px;"">
                                {Encode(heading)}
                            </h2>

                            <div style=""font-size:15px;
                                        line-height:1.7;
                                        color:#444444;"">

                                {content}

                            </div>

                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:20px 30px;
                                   background-color:#f8f8f8;
                                   font-size:13px;
                                   color:#777777;
                                   text-align:center;"">

                            Regards,<br />
                            <strong>HCC Technology Foundation</strong>

                        </td>
                    </tr>

                </table>

            </td>
        </tr>

    </table>

</body>
</html>";
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(
                value ?? string.Empty);
        }

        private static string FormatDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("dd MMMM yyyy")
                : "Not specified";
        }
    }
}