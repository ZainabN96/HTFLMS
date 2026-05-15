using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext db;

        public AssignmentService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await db.Assignments
                .Include(a => a.Course)
                .Include(a => a.Module)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Assignment>> GetAllAsync()
        {
            return await db.Assignments
                .Include(a => a.Course)
                .Include(a => a.Module)
                .Include(a => a.Submissions)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetByCourseIdAsync(int courseId)
        {
            return await db.Assignments
                .Include(a => a.Module)
                .Include(a => a.Submissions)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetByModuleIdAsync(int moduleId)
        {
            return await db.Assignments
                .Include(a => a.Module)
                .Include(a => a.Submissions)
                .Where(a => a.ModuleId == moduleId)
                .ToListAsync();
        }

        public void Add(Assignment assignment)
        {
            db.Assignments.Add(assignment);
        }

        public void Update(Assignment assignment)
        {
            db.Assignments.Update(assignment);
        }

        public void Delete(Assignment assignment)
        {
            db.Assignments.Remove(assignment);
        }
    }
}