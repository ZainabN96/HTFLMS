using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface ICourseService
    {
        Task<Course?> GetByIdAsync(int id);
        void Add(Course course);
        void Update(Course course);
        Task<List<Course>> GetAllAsync();
        Task<List<Course>> GetByTrainerIdAsync(int trainerId);
    }
}