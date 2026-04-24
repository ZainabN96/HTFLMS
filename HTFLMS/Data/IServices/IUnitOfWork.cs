namespace HTFLMS.Data.IServices
{
    public interface IUnitOfWork
    {
        IUserService UserService { get; }
        // IMailService MailService { get; }
        Task<bool> SaveAsync();
    }
}
