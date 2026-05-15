using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IModuleService
    {
        Task<Module?> GetByIdAsync(int id);

        Task<List<Module>> GetAllAsync();

        Task<List<Module>> GetByCourseIdAsync(int courseId);

        void Add(Module module);

        void Update(Module module);

        void Delete(Module module);
    }
}