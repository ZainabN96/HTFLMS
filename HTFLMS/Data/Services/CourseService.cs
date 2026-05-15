using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
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
                .Include(x => x.Trainer)
                .Include(x => x.Enrollments)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetByTrainerIdAsync(int trainerId)
        {
            return await context.Courses
                .Include(x => x.Enrollments)
                .Where(x => x.TrainerId == trainerId && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<StudentCourseDetailDto?> GetStudentCourseDetailAsync(int courseId)
        {
            return await context.Courses
                .Include(x => x.Trainer)
                .Where(x => x.Id == courseId)
                .Where(x => x.IsActive == true && x.IsPublished == true)
                .Select(x => new StudentCourseDetailDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Category = x.Category,
                    Description = x.Description,
                    HandbookFilePath = x.HandbookFilePath,
                    CourseImagePath = x.CourseImagePath,
                    BatchNumber = x.BatchNumber,
                    DurationText = x.DurationText,
                    BatchStartDate = x.BatchStartDate,
                    BatchEndDate = x.BatchEndDate,
                    CertificateIncluded = x.CertificateIncluded,
                    TrainerName = x.Trainer != null ? x.Trainer.Name : "No Trainer"
                })
                .FirstOrDefaultAsync();
        }

        public void Delete(Course course)
        {
            context.Courses.Remove(course);
        }
    }
}