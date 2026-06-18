using System.Threading.Tasks;
using HTFLMS.Dtos.StudentCourseContent;

namespace HTFLMS.Data.IServices
{
    public interface IStudentCourseContentService
    {
        Task<StudentCourseContentHeaderDto?> GetHeaderAsync(int studentId, int courseId);

        Task<StudentCourseContentInfoDto?> GetInfoAsync(int studentId, int courseId);
        Task<StudentCourseContentModulesDto?> GetModulesAndLessonsAsync(int studentId, int courseId);

        Task<bool> MarkLessonDoneAsync(int studentId, int lessonId);
        Task<StudentCourseContentQuizDto?> GetQuizAsync(int studentId, int moduleId);

        Task<StudentCourseContentQuizResultDto?> SubmitQuizAsync(
            int studentId,
            StudentCourseContentQuizSubmitDto dto);

        Task<StudentCourseContentQuizReviewDto?> GetQuizReviewAsync(int studentId, int quizId);
    }
}