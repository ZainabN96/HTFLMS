using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext context;

        public CourseService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await context.Courses.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void Add(Course course)
        {
            context.Courses.Add(course);
        }

        public void Update(Course course)
        {
            context.Courses.Update(course);
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await context.Courses
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetByTrainerIdAsync(int trainerId)
        {
            return await context.Courses
                .Where(x => x.TrainerId == trainerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public void Delete(Course course)
        {
            context.Courses.Remove(course);
        }
    }
}