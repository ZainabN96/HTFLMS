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
        Task<StudentCourseContentMaterialsAssignmentsDto?> GetMaterialsAndAssignmentsAsync(int studentId, int courseId);
        Task<StudentCourseContentAssignmentSubmitResultDto?> SubmitAssignmentAsync(
            int studentId,
            int assignmentId,
            IFormFile? file,
            string? solutionLink);

        Task<StudentCourseContentAssignmentUnsubmitResultDto?> UnsubmitAssignmentAsync(
            int studentId,
            int assignmentId);
        Task<StudentCourseContentNotesDto?> GetNotesAsync(int studentId, int courseId);

        Task<StudentCourseContentNoteActionResultDto?> CreateNoteAsync(
            int studentId,
            int courseId,
            StudentCourseContentNoteSaveDto dto);

        Task<StudentCourseContentNoteActionResultDto?> UpdateNoteAsync(
            int studentId,
            int noteId,
            StudentCourseContentNoteSaveDto dto);

        Task<StudentCourseContentNoteActionResultDto?> DeleteNoteAsync(
            int studentId,
            int noteId);
    }
}


