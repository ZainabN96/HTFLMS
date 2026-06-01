using HTFLMS.Dtos;
using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IManageStudentService
    {
        Task<List<ManageStudentListDto>> GetAllAsync();

        Task<User?> GetStudentByIdAsync(int id);

        Task<User?> GetStudentByEmailAsync(string email);

        Task<User?> GetAnyUserByEmailAsync(string email);

        Task<ManageStudentDto?> GetForEditAsync(int id);

        Task<List<int>> GetAllowedCourseIdsForUserAsync(string email);

        Task<int> GetNextStudentNumberAsync();
        Task<List<ManageStudentListDto>> GetAllForUserAsync(string email);

        void Add(User student);

        void Update(User student);

        Task EnrollStudentInCoursesAsync(int studentId, List<int> courseIds);
    }
}