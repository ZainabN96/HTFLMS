using System.Threading.Tasks;

namespace HTFLMS.Data.IServices
{
    public interface IMailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
