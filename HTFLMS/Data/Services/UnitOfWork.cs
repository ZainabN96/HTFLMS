using HTFLMS.Data.IServices;

namespace HTFLMS.Data.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public IUserService UserService { get; private set; }

        public ICourseService CourseService { get; private set; }

        public ICourseEnrollmentService CourseEnrollmentService { get; private set; }
        public IStudentDashboardService StudentDashboardService { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;

            UserService = new UserService(context);
            CourseService = new CourseService(context);
            CourseEnrollmentService = new CourseEnrollmentService(context);
            StudentDashboardService = new StudentDashboardService(context);
        }

        public async Task<bool> SaveAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}