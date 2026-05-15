using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IQuizService
    {
        Task<Quiz?> GetByIdAsync(int id);

        Task<List<Quiz>> GetAllAsync();

        Task<List<Quiz>> GetByModuleIdAsync(int moduleId);

        void Add(Quiz quiz);

        void Update(Quiz quiz);

        void Delete(Quiz quiz);
    }
}