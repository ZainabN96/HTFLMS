using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext db;

        public LessonService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Lesson?> GetByIdAsync(int id)
        {
            return await db.Lessons
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Lesson>> GetAllAsync()
        {
            return await db.Lessons
                .Include(l => l.Module)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetByModuleIdAsync(int moduleId)
        {
            return await db.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync();
        }

        public void Add(Lesson lesson)
        {
            db.Lessons.Add(lesson);
        }

        public void Update(Lesson lesson)
        {
            db.Lessons.Update(lesson);
        }

        public void Delete(Lesson lesson)
        {
            db.Lessons.Remove(lesson);
        }
    }
}