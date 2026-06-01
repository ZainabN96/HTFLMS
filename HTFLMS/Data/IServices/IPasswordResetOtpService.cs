using HTFLMS.Models.Auth;

namespace HTFLMS.Data.IServices
{
    public interface IPasswordResetOtpService
    {
        Task<PasswordResetOtp?> GetByFlowIdAsync(string flowId);
        Task<PasswordResetOtp?> GetLatestActiveByEmailAsync(string email);
        void Add(PasswordResetOtp otp);
        void Update(PasswordResetOtp otp);
    }
}