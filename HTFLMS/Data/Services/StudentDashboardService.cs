using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly ApplicationDbContext context;

        public StudentDashboardService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(int studentId)
        {
            var student = await context.Users
                .FirstOrDefaultAsync(x => x.Id == studentId && x.MemberType == "Student");

            if (student == null)
            {
                return new StudentDashboardDto();
            }

            var enrollments = await context.CourseEnrollments
                .Include(x => x.Course)
                    .ThenInclude(c => c.Trainer)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.EnrolledAt)
                .ToListAsync();

            var enrolledCourses = enrollments
                .Where(x => x.Course != null)
                .Select(x => new StudentEnrolledCoursesDto
                {
                    CourseId = x.CourseId,
                    Title = x.Course!.Title,
                    Category = x.Course.Category,
                    TrainerName = x.Course.Trainer != null ? x.Course.Trainer.Name : "No Trainer",
                    CourseImagePath = x.Course.CourseImagePath,
                    BatchStartDate = x.Course.BatchStartDate,
                    BatchEndDate = x.Course.BatchEndDate,
                    EnrollmentStatus = x.Status,
                    ProgressPercentage = 0
                })
                .ToList();

            return new StudentDashboardDto
            {
                StudentName = student.Name,
                TotalEnrolledCourses = enrollments.Count,
                ActiveCourseCount = enrollments.Count(x => x.Status == "Active"),
                CompletedCourseCount = enrollments.Count(x => x.Status == "Completed"),

                // Abhi trainer-side lesson/grade/assignment ka kaam final nahi,
                // is liye safe default 0 rakha hai.
                LessonsCompleted = 0,
                AverageGrade = 0,
                PendingTasks = 0,

                EnrolledCourses = enrolledCourses
            };
        }
    }
}