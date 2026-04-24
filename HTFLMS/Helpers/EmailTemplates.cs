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

                    <p>Your account has been created successfully in <b>HTF LMS</b>. 
                    We are thrilled to have you as part of our community.</p>

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

                    <p>Please keep your credentials confidential and do not share them with anyone.</p>

                    <p>If you have any questions, feel free to contact our support team.</p>

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
    }
}