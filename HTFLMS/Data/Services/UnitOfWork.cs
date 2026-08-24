using HTFLMS.Data.IServices;
using HTFLMS.Data.Services;
using Microsoft.EntityFrameworkCore;

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
        public ICourseEnrollmentService CourseEnrollmentService { get; }
        public IStudentDashboardService StudentDashboardService { get; }
        public IManageTrainerService ManageTrainerService { get; }
        public IPasswordResetOtpService PasswordResetOtpService { get; }
        public IManageStudentService ManageStudentService { get; }

        public IStudentGradesService StudentGradesService { get; }
        public IStudentCourseContentService StudentCourseContentService { get; }
        public ITrainerAssignmentGradingService TrainerAssignmentGradingService { get; }
        public IStudentCertificateService StudentCertificateService { get; }
        public ITrainerCertificateService TrainerCertificateService { get; }
        public IAdminCertificateService AdminCertificateService { get; }
        public ICertificateGenerationService CertificateGenerationService { get; }

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
            CourseEnrollmentService = new CourseEnrollmentService(context);
            StudentDashboardService = new StudentDashboardService(context);
            ManageTrainerService = new ManageTrainerService(context);
            PasswordResetOtpService = new PasswordResetOtpService(context);
            ManageStudentService = new ManageStudentService(context);

            StudentGradesService = new StudentGradesService(context);
            StudentCourseContentService = new StudentCourseContentService(context);
            TrainerAssignmentGradingService = new TrainerAssignmentGradingService(context);
            StudentCertificateService = new StudentCertificateService(context);
            TrainerCertificateService = new TrainerCertificateService(context);
            AdminCertificateService = new AdminCertificateService(context);
            CertificateGenerationService = new CertificateGenerationService(context);
        }

        public async Task<bool> SaveAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}