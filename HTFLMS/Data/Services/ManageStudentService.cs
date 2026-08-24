using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class ManageStudentService : IManageStudentService
    {
        private readonly ApplicationDbContext context;

        public ManageStudentService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<ManageStudentListDto>> GetAllAsync()
        {
            return await context.Users
                .Where(x => x.MemberType == "Student")
                .Include(x => x.Enrollments!)
                    .ThenInclude(e => e.Course)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ManageStudentListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Status = x.IsActive ? "Active" : "Inactive",
                    CreatedAt = x.CreatedAt,

                    EnrolledCoursesCount = x.Enrollments == null
                        ? 0
                        : x.Enrollments.Count(e => e.Status == "Active"),

                    AverageGrade = 0,

                    EnrolledCourses = x.Enrollments == null
                        ? new List<string>()
                        : x.Enrollments
                            .Where(e => e.Status == "Active" && e.Course != null)
                            .Select(e => e.Course!.Title)
                            .ToList()
                })
                .ToListAsync();
        }

        public async Task<User?> GetStudentByIdAsync(int id)
        {
            return await context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.MemberType == "Student");
        }

        public async Task<User?> GetStudentByEmailAsync(string email)
        {
            return await context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.MemberType == "Student");
        }

        public async Task<User?> GetAnyUserByEmailAsync(string email)
        {
            return await context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<ManageStudentDto?> GetForEditAsync(int id)
        {
            var student = await context.Users
                .Include(x => x.Enrollments)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.MemberType == "Student");

            if (student == null)
                return null;

            return new ManageStudentDto
            {
                Id = student.Id,
                TitlePrefix = student.TitlePrefix ?? "",
                Name = student.Name,
                Email = student.Email,
                Password = "",
                Status = student.IsActive ? "Active" : "Inactive",
                JoinDate = student.CreatedAt,

                CourseIds = student.Enrollments == null
                    ? new List<int>()
                    : student.Enrollments
                        .Where(x => x.Status == "Active")
                        .Select(x => x.CourseId)
                        .ToList()
            };
        }

        public void Add(User student)
        {
            context.Users.Add(student);
        }

        public void Update(User student)
        {
            context.Users.Update(student);
        }

        public async Task EnrollStudentInCoursesAsync(int studentId, List<int> courseIds)
        {
            if (courseIds == null || courseIds.Count == 0)
                return;

            var deliveryMode = await GetDefaultDeliveryModeForStudentAsync(studentId);

            var validCourseIds = await context.Courses
                .Where(x =>
                    courseIds.Contains(x.Id) &&
                    x.IsActive == true &&
                    x.IsPublished == true)
                .Select(x => x.Id)
                .ToListAsync();

            var existingEnrollments = await context.CourseEnrollments
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            foreach (var courseId in validCourseIds.Distinct())
            {
                var existing = existingEnrollments
                    .FirstOrDefault(x => x.CourseId == courseId);

                if (existing != null)
                {
                    existing.Status = "Active";
                    existing.DroppedAt = null;
                    existing.CompletedAt = null;

                    if (string.IsNullOrWhiteSpace(existing.DeliveryMode))
                    {
                        existing.DeliveryMode = deliveryMode;
                    }
                }
                else
                {
                    context.CourseEnrollments.Add(new CourseEnrollment
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        EnrolledAt = DateTime.UtcNow,
                        Status = "Active",
                        DeliveryMode = deliveryMode
                    });
                }
            }
        }

        public async Task<List<int>> GetAllowedCourseIdsForUserAsync(string email)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return new List<int>();

            if (string.Equals(user.MemberType, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return await context.Courses
                    .Where(x => x.IsActive == true && x.IsPublished == true)
                    .Select(x => x.Id)
                    .ToListAsync();
            }

            if (string.Equals(user.MemberType, "Trainer", StringComparison.OrdinalIgnoreCase))
            {
                return await context.Courses
                    .Where(x =>
                        x.TrainerId == user.Id &&
                        x.IsActive == true &&
                        x.IsPublished == true)
                    .Select(x => x.Id)
                    .ToListAsync();
            }

            return new List<int>();
        }

        public async Task<int> GetNextStudentNumberAsync()
        {
            var count = await context.Users
                .CountAsync(x => x.MemberType == "Student");

            return count + 1;
        }

        public async Task<List<ManageStudentListDto>> GetAllForUserAsync(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return new List<ManageStudentListDto>();

            if (string.Equals(user.MemberType, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return await GetAllAsync();
            }

            if (string.Equals(user.MemberType, "Trainer", StringComparison.OrdinalIgnoreCase))
            {
                return await context.Users
                    .Where(x => x.MemberType == "Student")
                    .Include(x => x.Enrollments!)
                        .ThenInclude(e => e.Course)
                    .Where(x => x.Enrollments != null &&
                                x.Enrollments.Any(e =>
                                    e.Status == "Active" &&
                                    e.Course != null &&
                                    e.Course.TrainerId == user.Id))
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new ManageStudentListDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        Status = x.IsActive ? "Active" : "Inactive",
                        CreatedAt = x.CreatedAt,

                        EnrolledCoursesCount = x.Enrollments == null
                            ? 0
                            : x.Enrollments.Count(e =>
                                e.Status == "Active" &&
                                e.Course != null &&
                                e.Course.TrainerId == user.Id),

                        AverageGrade = 0,

                        EnrolledCourses = x.Enrollments == null
                            ? new List<string>()
                            : x.Enrollments
                                .Where(e =>
                                    e.Status == "Active" &&
                                    e.Course != null &&
                                    e.Course.TrainerId == user.Id)
                                .Select(e => e.Course!.Title)
                                .ToList()
                    })
                    .ToListAsync();
            }

            return new List<ManageStudentListDto>();
        }

        public async Task UpdateStudentCourseEnrollmentsAsync(int studentId, List<int> selectedCourseIds)
        {
            selectedCourseIds ??= new List<int>();

            var deliveryMode = await GetDefaultDeliveryModeForStudentAsync(studentId);

            var existingEnrollments = await context.CourseEnrollments
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var existingCourseIds = existingEnrollments
                .Select(x => x.CourseId)
                .ToList();

            var enrollmentsToRemove = existingEnrollments
                .Where(x => !selectedCourseIds.Contains(x.CourseId))
                .ToList();

            context.CourseEnrollments.RemoveRange(enrollmentsToRemove);

            var courseIdsToAdd = selectedCourseIds
                .Where(courseId => !existingCourseIds.Contains(courseId))
                .ToList();

            foreach (var courseId in courseIdsToAdd)
            {
                context.CourseEnrollments.Add(new CourseEnrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = "Active",
                    DeliveryMode = deliveryMode
                });
            }

            var selectedExistingEnrollments = existingEnrollments
                .Where(x => selectedCourseIds.Contains(x.CourseId))
                .ToList();

            foreach (var enrollment in selectedExistingEnrollments)
            {
                if (string.IsNullOrWhiteSpace(enrollment.DeliveryMode))
                {
                    enrollment.DeliveryMode = deliveryMode;
                }
            }
        }

        private async Task<string> GetDefaultDeliveryModeForStudentAsync(int studentId)
        {
            var city = await context.Users
                .Where(x => x.Id == studentId)
                .Select(x => x.City)
                .FirstOrDefaultAsync();

            return GetDefaultDeliveryMode(city);
        }

        private static string GetDefaultDeliveryMode(string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return "Onsite";

            return string.Equals(city.Trim(), "Lahore", StringComparison.OrdinalIgnoreCase)
                ? "Onsite"
                : "Online";
        }
    }
}