using HTFLMS.Data.IServices;

namespace HTFLMS.Data.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public IUserService UserService { get; private set; }
        public ICourseService CourseService { get; private set; }
        public IModuleService ModuleService { get; private set; }
        public ILessonService LessonService { get; }
        public IQuizService QuizService { get; }
        public IMaterialService MaterialService { get; }
        public IAssignmentService AssignmentService { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;

            UserService = new UserService(context);
            CourseService = new CourseService(context);
            ModuleService = new ModuleService(context);
            LessonService = new LessonService(context);
            QuizService = new QuizService(context);
            MaterialService = new MaterialService(context);
            AssignmentService = new AssignmentService(context);
        }

        public async Task<bool> SaveAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}