using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface ILessonService
    {
        Task<Lesson?> GetByIdAsync(int id);

        Task<List<Lesson>> GetAllAsync();

        Task<List<Lesson>> GetByModuleIdAsync(int moduleId);

        void Add(Lesson lesson);

        void Update(Lesson lesson);

        void Delete(Lesson lesson);
    }
}