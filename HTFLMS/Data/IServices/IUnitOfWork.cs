namespace HTFLMS.Data.IServices
{
    public interface IUnitOfWork
    {
        IUserService UserService { get; }
        ICourseService CourseService { get; }
        IModuleService ModuleService { get; }
        ILessonService LessonService { get; }
        IQuizService QuizService { get; }
        IMaterialService MaterialService { get; }
        IAssignmentService AssignmentService { get; }
        ICourseEnrollmentService CourseEnrollmentService { get; }
        IStudentDashboardService StudentDashboardService { get; }
        IManageTrainerService ManageTrainerService { get; }
        IPasswordResetOtpService PasswordResetOtpService { get; }
        IManageStudentService ManageStudentService { get; }
        IStudentGradesService StudentGradesService { get; }
        IStudentCourseContentService StudentCourseContentService { get; }
        ITrainerAssignmentGradingService TrainerAssignmentGradingService { get; }
        IStudentCertificateService StudentCertificateService { get; }
        ITrainerCertificateService TrainerCertificateService { get; }
        IAdminCertificateService AdminCertificateService { get; }
        Task<bool> SaveAsync();
    }
}