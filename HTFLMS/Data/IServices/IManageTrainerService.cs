using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IManageTrainerService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);

        Task<bool> EmailExistsAsync(string email, int? ignoreId = null);
        Task<bool> CnicExistsAsync(string cnic, int? ignoreId = null);

        Task<int> GetAssignedCourseCountAsync(int trainerId);
        Task<string> GenerateUniqueUserIdAsync();

        void Add(User trainer);
        void Update(User trainer);
        void Delete(User trainer);
    }
}