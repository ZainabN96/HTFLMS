using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IAssignmentService
    {
        Task<Assignment?> GetByIdAsync(int id);

        Task<List<Assignment>> GetAllAsync();

        Task<List<Assignment>> GetByCourseIdAsync(int courseId);

        Task<List<Assignment>> GetByModuleIdAsync(int moduleId);

        void Add(Assignment assignment);

        void Update(Assignment assignment);

        void Delete(Assignment assignment);
    }
}