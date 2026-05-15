using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext db;

        public MaterialService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Material?> GetByIdAsync(int id)
        {
            return await db.Materials
                .Include(m => m.Course)
                .Include(m => m.Module)
                .Include(m => m.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Material>> GetAllAsync()
        {
            return await db.Materials
                .Include(m => m.Course)
                .Include(m => m.Module)
                .Include(m => m.Lesson)
                .ToListAsync();
        }

        public async Task<List<Material>> GetByCourseIdAsync(int courseId)
        {
            return await db.Materials
                .Include(m => m.Module)
                .Where(m => m.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<List<Material>> GetByModuleIdAsync(int moduleId)
        {
            return await db.Materials
                .Include(m => m.Module)
                .Where(m => m.ModuleId == moduleId)
                .ToListAsync();
        }

        public void Add(Material material)
        {
            db.Materials.Add(material);
        }

        public void Update(Material material)
        {
            db.Materials.Update(material);
        }

        public void Delete(Material material)
        {
            db.Materials.Remove(material);
        }
    }
}