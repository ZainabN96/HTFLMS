using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class CourseEnrollmentService
        : ICourseEnrollmentService
    {
        private readonly ApplicationDbContext context;

        public CourseEnrollmentService(
            ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> IsAlreadyEnrolledAsync(
            int studentId,
            int courseId)
        {
            return await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.CourseId == courseId &&
                    x.Status == "Active");
        }

        public async Task<bool> HasAnyActiveEnrollmentAsync(
            int studentId)
        {
            return await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.Status == "Active");
        }

        public async Task<List<int>> GetActiveStudentIdsByCourseAsync(
            int courseId)
        {
            return await context.CourseEnrollments
                .Where(x =>
                    x.CourseId == courseId &&
                    x.Status == "Active")
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();
        }

        public void Add(
            CourseEnrollment enrollment)
        {
            context.CourseEnrollments.Add(enrollment);
        }
    }
}