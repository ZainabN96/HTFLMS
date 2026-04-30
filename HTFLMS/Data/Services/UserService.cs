using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext dc;
        public UserService(ApplicationDbContext dc)
        {
            this.dc = dc;
        }
        public async Task<bool> UserAlreadyExists(string cnic, string email)
        {
            return await dc.Users.AnyAsync(u => u.CNIC == cnic || u.Email == email);
        }
        public async Task<bool> UserIdExists(string userId)
        {
            return await dc.Users.AnyAsync(u => u.UserId == userId);
        }
        public void Register(User user)
        {
            dc.Users.Add(user);
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await dc.Users.FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}
