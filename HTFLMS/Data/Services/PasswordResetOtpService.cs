using HTFLMS.Data.IServices;
using HTFLMS.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class PasswordResetOtpService : IPasswordResetOtpService
    {
        private readonly ApplicationDbContext context;

        public PasswordResetOtpService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<PasswordResetOtp?> GetByFlowIdAsync(string flowId)
        {
            return await context.PasswordResetOtps
                .FirstOrDefaultAsync(x => x.FlowId == flowId);
        }

        public async Task<PasswordResetOtp?> GetLatestActiveByEmailAsync(string email)
        {
            return await context.PasswordResetOtps
                .Where(x =>
                    x.Email == email &&
                    x.IsUsed == false &&
                    x.ExpiresAtUtc > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        public void Add(PasswordResetOtp otp)
        {
            context.PasswordResetOtps.Add(otp);
        }

        public void Update(PasswordResetOtp otp)
        {
            context.PasswordResetOtps.Update(otp);
        }
    }
}