namespace HTFLMS.Data.IServices
{
    public interface IUnitOfWork
    {
        IUserService UserService { get; }
        ICourseService CourseService { get; }

        Task<bool> SaveAsync();
    }
}