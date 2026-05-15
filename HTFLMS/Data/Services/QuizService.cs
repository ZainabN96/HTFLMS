using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext db;

        public QuizService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Quiz?> GetByIdAsync(int id)
        {
            return await db.Quizzes
                .Include(q => q.Module)
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Quiz>> GetAllAsync()
        {
            return await db.Quizzes
                .Include(q => q.Module)
                .Include(q => q.Questions)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetByModuleIdAsync(int moduleId)
        {
            return await db.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.ModuleId == moduleId)
                .ToListAsync();
        }

        public void Add(Quiz quiz)
        {
            db.Quizzes.Add(quiz);
        }

        public void Update(Quiz quiz)
        {
            db.Quizzes.Update(quiz);
        }

        public void Delete(Quiz quiz)
        {
            db.Quizzes.Remove(quiz);
        }
    }
}