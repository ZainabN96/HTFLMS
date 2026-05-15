using HTFLMS.Models;

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

        Task<bool> SaveAsync();
    }
}