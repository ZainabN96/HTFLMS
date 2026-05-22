using HTFLMS.Dtos;

namespace HTFLMS.Data.IServices
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardDto> GetDashboardAsync(int studentId);
    }
}