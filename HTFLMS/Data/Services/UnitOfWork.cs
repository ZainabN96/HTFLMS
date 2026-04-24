using HTFLMS.Data.IServices;

namespace HTFLMS.Data.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dc;

        public UnitOfWork(ApplicationDbContext dc)
        {
            this.dc = dc;
        }
        IUserService IUnitOfWork.UserService => new UserService(dc);
        //public IMailService MailService => new MailService(dc);
        public async Task<bool> SaveAsync()
        {
            try
            {
                return await dc.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                throw new Exception(message, ex);
            }
        }
    }
}
