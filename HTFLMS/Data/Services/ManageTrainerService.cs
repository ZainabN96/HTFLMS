using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class ManageTrainerService : IManageTrainerService
    {
        private readonly ApplicationDbContext db;

        public ManageTrainerService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await db.Users
                .Where(u => u.MemberType == "Trainer")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await db.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.MemberType == "Trainer");
        }

        public async Task<bool> EmailExistsAsync(string email, int? ignoreId = null)
        {
            return await db.Users.AnyAsync(u =>
                u.Email == email &&
                (!ignoreId.HasValue || u.Id != ignoreId.Value));
        }

        public async Task<bool> CnicExistsAsync(string cnic, int? ignoreId = null)
        {
            return await db.Users.AnyAsync(u =>
                u.CNIC == cnic &&
                (!ignoreId.HasValue || u.Id != ignoreId.Value));
        }

        public async Task<int> GetAssignedCourseCountAsync(int trainerId)
        {
            return await db.Courses.CountAsync(c => c.TrainerId == trainerId);
        }

        public async Task<string> GenerateUniqueUserIdAsync()
        {
            string userId;

            do
            {
                userId = "TRN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }
            while (await db.Users.AnyAsync(u => u.UserId == userId));

            return userId;
        }

        public void Add(User trainer)
        {
            db.Users.Add(trainer);
        }

        public void Update(User trainer)
        {
            db.Users.Update(trainer);
        }

        public void Delete(User trainer)
        {
            db.Users.Remove(trainer);
        }
    }
}