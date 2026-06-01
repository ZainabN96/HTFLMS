// HTFLMS/Helper/EmailTemplates.cs
namespace HTFLMS.Helper
{
    public static class EmailTemplates
    {
        public static (string Subject, string Body) RegistrationEmail(
            string name, string userId, string password)
        {
            var subject = "Welcome to HTF LMS — Your Account Credentials";

            var body = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto;'>

                <div style='background-color: #003366; padding: 20px; text-align: center;'>
                    <h2 style='color: white; margin: 0;'>HCC Tech Foundation LMS</h2>
                </div>

                <div style='padding: 30px;'>
                    <p>Hi <b>{name}</b>,</p>

                    <p>Your account has been created successfully in <b>HTF LMS</b>.</p>

                    <p>Here are your login credentials:</p>

                    <table style='border-collapse: collapse; width: 100%; margin: 20px 0;'>
                        <tr style='background-color: #f2f2f2;'>
                            <td style='padding: 10px; border: 1px solid #ddd; font-weight: bold;'>User ID</td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{userId}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd; font-weight: bold;'>Password</td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{password}</td>
                        </tr>
                    </table>

                    <p>Please keep your credentials confidential.</p>

                    <br>
                    <p>Best regards,<br>
                    <b>HTF LMS Team</b><br>
                    HCC Tech Foundation</p>
                </div>

                <div style='background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; color: #888;'>
                    &copy; {DateTime.UtcNow.Year} HCC Tech Foundation. All rights reserved.
                </div>

            </body>
            </html>";

            return (subject, body);
        }

        public static (string Subject, string Body) PasswordResetOtpEmail(string otp)
        {
            var subject = "HTF LMS - Password Reset OTP";

            var body = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto;'>

                <div style='background-color: #c2343d; padding: 20px; text-align: center;'>
                    <h2 style='color: white; margin: 0;'>HTF LMS Password Reset</h2>
                </div>

                <div style='padding: 30px;'>
                    <p>Hello,</p>

                    <p>You requested to reset your HTF LMS password.</p>

                    <p>Your OTP code is:</p>

                    <h1 style='letter-spacing: 5px; color: #c2343d; text-align: center;'>
                        {otp}
                    </h1>

                    <p>This OTP will expire in <b>10 minutes</b>.</p>

                    <p>If you did not request this password reset, please ignore this email.</p>

                    <br>
                    <p>Best regards,<br>
                    <b>HTF LMS Team</b><br>
                    HCC Tech Foundation</p>
                </div>

                <div style='background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; color: #888;'>
                    &copy; {DateTime.UtcNow.Year} HCC Tech Foundation. All rights reserved.
                </div>

            </body>
            </html>";

            return (subject, body);
        }

        public static (string Subject, string Body) ContactMessageEmail(
            string name, string email, string subjectText, string message)
        {
            var subject = $"HTF LMS Contact Form - {subjectText}";

            var body = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto;'>

                <div style='background-color: #003366; padding: 20px; text-align: center;'>
                    <h2 style='color: white; margin: 0;'>New Contact Message</h2>
                </div>

                <div style='padding: 30px;'>
                    <p><b>Name:</b> {name}</p>
                    <p><b>Email:</b> {email}</p>
                    <p><b>Subject:</b> {subjectText}</p>

                    <hr>

                    <p><b>Message:</b></p>
                    <p>{message}</p>
                </div>

                <div style='background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; color: #888;'>
                    &copy; {DateTime.UtcNow.Year} HCC Tech Foundation. All rights reserved.
                </div>

            </body>
            </html>";

            return (subject, body);
        }
    }
}