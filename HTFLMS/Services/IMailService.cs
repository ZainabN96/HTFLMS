using System.Threading.Tasks;

namespace HTFLMS.Services
{
    public interface IMailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
