using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class ModuleService : IModuleService
    {
        private readonly ApplicationDbContext context;

        public ModuleService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Module?> GetByIdAsync(int id)
        {
            return await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Module>> GetAllAsync()
        {
            return await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Module>> GetByCourseIdAsync(int courseId)
        {
            return await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz)
                .Where(x => x.CourseId == courseId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public void Add(Module module)
        {
            context.Modules.Add(module);
        }

        public void Update(Module module)
        {
            context.Modules.Update(module);
        }

        public void Delete(Module module)
        {
            context.Modules.Remove(module);
        }
    }
}