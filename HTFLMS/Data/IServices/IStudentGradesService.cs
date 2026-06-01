using HTFLMS.DTOs.StudentGrades;

namespace HTFLMS.Data.IServices
{
    public interface IStudentGradesService
    {
        Task<GradesPageDto> GetGradesPageAsync(int studentId);
    }
}