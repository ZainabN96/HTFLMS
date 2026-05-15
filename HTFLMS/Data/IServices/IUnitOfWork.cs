namespace HTFLMS.Data.IServices
{
    public interface IUnitOfWork
    {
        IUserService UserService { get; }
        ICourseService CourseService { get; }
        ICourseEnrollmentService CourseEnrollmentService { get; }
        IStudentDashboardService StudentDashboardService { get; }

        Task<bool> SaveAsync();
    }
}