namespace HTFLMS.Data.IServices
{
    public interface IMailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);

        Task SendRegistrationEmailAsync(
            string toEmail,
            string name,
            string userId,
            string password
        );
    }
}