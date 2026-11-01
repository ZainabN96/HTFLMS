
using System;
using System.Net;

namespace HTFLMS.Helper
{
    public static class EmailTemplates
    {
        private const string ThemeColor = "#C9828F";

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? "");
        }

        private static string BuildEmail(
            string heading,
            string content)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin:0; padding:0; background-color:#f7f4f5; font-family:Arial,Helvetica,sans-serif; color:#333333;'>

    <table role='presentation' width='100%' cellpadding='0' cellspacing='0'
           style='background-color:#f7f4f5; padding:30px 12px;'>
        <tr>
            <td align='center'>

                <table role='presentation' width='100%' cellpadding='0' cellspacing='0'
                       style='max-width:600px; background-color:#ffffff; border:1px solid #e9e0e3;'>

                    <!-- Header -->
                    <tr>
                        <td align='center'
                            style='background-color:{ThemeColor}; padding:28px 20px;'>

                            <h2 style='color:#ffffff; margin:0; font-size:22px; font-weight:700;'>
                                HCC Technology Foundation
                            </h2>

                            <p style='color:#ffffff; margin:9px 0 0; font-size:12px; letter-spacing:1.2px;'>
                                LEARNING MANAGEMENT SYSTEM
                            </p>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td style='padding:35px 30px; font-size:14px; line-height:1.8;'>

                            <h1 style='color:#28242a; margin:0 0 24px; font-size:24px; line-height:1.4;'>
                                {Encode(heading)}
                            </h1>

                            {content}

                            <p style='margin-top:30px; margin-bottom:0;'>
                                Best regards,<br>
                                <strong>HCC Technology Foundation Team</strong>
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td align='center'
                            style='background-color:#f7f4f5; padding:22px 15px; border-top:1px solid #e9e0e3;'>

                            <p style='margin:0 0 6px; font-size:13px; color:#555555;'>
                                HCC Technology Foundation
                            </p>

                            <p style='margin:0; font-size:11px; color:#888888;'>
                                &copy; {DateTime.UtcNow.Year} HCC Technology Foundation.
                                All rights reserved.
                            </p>

                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
        }

        // REGISTRATION EMAIL
        public static (string Subject, string Body) RegistrationEmail(
            string name,
            string userId)
        {
            var subject = "Welcome to HTF LMS | Your Account Is Ready";

            var content = $@"
                <p>Dear <strong>{Encode(name)}</strong>,</p>

                <p>
                    Welcome to HCC Technology Foundation's
                    Learning Management System!
                </p>

                <p>
                    Your account has been successfully created.
                    You can now access your learning dashboard,
                    explore available courses, and begin your
                    learning journey with us.
                </p>

                <table role='presentation' width='100%' cellpadding='0'
                       cellspacing='0'
                       style='background-color:#f9f3f5; margin:25px 0;'>
                    <tr>
                        <td style='padding:18px 22px;'>
                            <p style='margin:0 0 5px; color:#90606b; font-size:12px; font-weight:bold;'>
                                YOUR USER ID
                            </p>

                            <p style='margin:0; color:#28242a; font-size:22px; font-weight:bold;'>
                                {Encode(userId)}
                            </p>
                        </td>
                    </tr>
                </table>

                <p>
                    To sign in, use your registered email address
                    and the password you created during registration.
                </p>



                <p>
                    We look forward to supporting you throughout
                    your learning journey.
                </p>";

            return (
                subject,
                BuildEmail("Welcome to HTF LMS!", content)
            );
        }

        // PASSWORD RESET OTP EMAIL
        public static (string Subject, string Body) PasswordResetOtpEmail(
            string otp)
        {
            var subject = "HTF LMS | Password Reset Verification Code";

            var content = $@"
                <p>Hello,</p>

                <p>
                    We received a request to reset the password
                    for your HTF LMS account.
                </p>

                <p>
                    Please use the following verification code
                    to continue:
                </p>

                <table role='presentation' width='100%' cellpadding='0'
                       cellspacing='0'
                       style='background-color:#f9f3f5; margin:25px 0;'>
                    <tr>
                        <td align='center' style='padding:25px 15px;'>

                            <p style='margin:0; color:{ThemeColor}; font-size:30px; font-weight:bold; letter-spacing:5px;'>
                                {Encode(otp)}
                            </p>

                        </td>
                    </tr>
                </table>

                <p>
                    This verification code will expire in
                    <strong>10 minutes</strong>.
                </p>

                <p style='color:#666666; font-size:13px;'>
                    For your security, do not share this code
                    with anyone.
                </p>

                <p>
                    If you did not request a password reset,
                    you can safely ignore this email.
                </p>";

            return (
                subject,
                BuildEmail("Password Reset Request", content)
            );
        }

        // CONTACT FORM EMAIL
        public static (string Subject, string Body) ContactMessageEmail(
            string name,
            string email,
            string subjectText,
            string message)
        {
            var subject = $"HTF LMS Contact Form - {subjectText}";

            var safeMessage = Encode(message)
                .Replace("\r\n", "\n")
                .Replace("\n", "<br>");

            var content = $@"
                <p>
                    A new message has been submitted through
                    the HTF LMS contact form.
                </p>

                <table role='presentation' width='100%' cellpadding='0'
                       cellspacing='0'
                       style='margin:25px 0; border-collapse:collapse;'>

                    <tr>
                        <td style='padding:12px; background-color:#f9f3f5; font-weight:bold; width:110px;'>
                            Name
                        </td>
                        <td style='padding:12px; background-color:#f9f3f5;'>
                            {Encode(name)}
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:12px; font-weight:bold;'>
                            Email
                        </td>
                        <td style='padding:12px;'>
                            {Encode(email)}
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:12px; background-color:#f9f3f5; font-weight:bold;'>
                            Subject
                        </td>
                        <td style='padding:12px; background-color:#f9f3f5;'>
                            {Encode(subjectText)}
                        </td>
                    </tr>

                </table>

                <p style='font-weight:bold; margin-bottom:8px;'>
                    Message:
                </p>

                <div style='background-color:#f9f3f5; padding:18px; border-left:3px solid {ThemeColor}; overflow-wrap:break-word;'>
                    {safeMessage}
                </div>";

            return (
                subject,
                BuildEmail("New Contact Message", content)
            );
        }
    }
}
