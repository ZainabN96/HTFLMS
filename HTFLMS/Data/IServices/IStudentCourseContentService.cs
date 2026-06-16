using HTFLMS.Dtos.StudentCourseContent;

namespace HTFLMS.Data.IServices
{
    public interface IStudentCourseContentService
    {
        Task<StudentCourseContentHeaderDto?> GetHeaderAsync(int studentId, int courseId);

        Task<StudentCourseContentInfoDto?> GetInfoAsync(int studentId, int courseId);
        Task<StudentCourseContentModulesDto?> GetModulesAndLessonsAsync(int studentId, int courseId);

        Task<bool> MarkLessonDoneAsync(int studentId, int lessonId);
    }
}