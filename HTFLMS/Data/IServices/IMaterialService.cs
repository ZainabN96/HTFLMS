using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IMaterialService
    {
        Task<Material?> GetByIdAsync(int id);

        Task<List<Material>> GetAllAsync();

        Task<List<Material>> GetByCourseIdAsync(int courseId);

        Task<List<Material>> GetByModuleIdAsync(int moduleId);

        void Add(Material material);

        void Update(Material material);

        void Delete(Material material);
    }
}