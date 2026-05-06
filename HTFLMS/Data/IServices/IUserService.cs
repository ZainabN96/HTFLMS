using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface IUserService
    {
        Task<bool> UserAlreadyExists(string cnic, string email);
        Task<bool> UserIdExists(string userId);
        void Register(Models.User user);
        Task<User?> GetUserByEmailAsync(string email);

        //trainer list for dropdown by sb
        Task<List<User>> GetAllAsync();
    }
}
