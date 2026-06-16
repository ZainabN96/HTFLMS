using HTFLMS.Data.IServices;
using HTFLMS.Dtos.StudentCourseContent;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentCourseContentService : IStudentCourseContentService
    {
        private readonly ApplicationDbContext context;

        public StudentCourseContentService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<StudentCourseContentHeaderDto?> GetHeaderAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var progress = await CalculateCourseProgressAsync(studentId, courseId);

            return new StudentCourseContentHeaderDto
            {
                CourseId = course.Id,
                Title = course.Title,
                CourseImagePath = course.CourseImagePath,
                TrainerName = course.Trainer?.Name ?? "No Trainer",
                CertificateIncluded = course.CertificateIncluded,
                ProgressPercentage = progress
            };
        }

        public async Task<StudentCourseContentInfoDto?> GetInfoAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var visibleModules = await GetVisibleModulesAsync(courseId);

            var totalLessons = visibleModules
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Count(l => l.IsActive);

            return new StudentCourseContentInfoDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Category = course.Category,
                Description = course.Description,
                CourseImagePath = course.CourseImagePath,
                TrainerName = course.Trainer?.Name ?? "No Trainer",
                CertificateIncluded = course.CertificateIncluded,
                BatchStartDate = course.BatchStartDate,
                BatchEndDate = course.BatchEndDate,
                DurationText = course.DurationText,
                TotalModules = visibleModules.Count,
                TotalLessons = totalLessons
            };
        }

        private async Task<Course?> GetAllowedCourseAsync(int studentId, int courseId)
        {
            var isEnrolled = await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.CourseId == courseId &&
                    x.Status == "Active");

            if (!isEnrolled)
            {
                return null;
            }

            return await context.Courses
                .Include(x => x.Trainer)
                .FirstOrDefaultAsync(x =>
                    x.Id == courseId &&
                    x.IsActive == true &&
                    x.IsPublished == true);
        }

        private async Task<List<Module>> GetVisibleModulesAsync(int courseId)
        {
            var modules = await context.Modules
                .Include(x => x.Lessons)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.IsActive == true)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return modules
                .Where(m => m.Lessons != null && m.Lessons.Any(l => l.IsActive))
                .ToList();
        }

        private async Task<int> CalculateCourseProgressAsync(int studentId, int courseId)
        {
            var visibleModules = await GetVisibleModulesAsync(courseId);

            var activeLessons = visibleModules
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Where(l => l.IsActive)
                .ToList();

            if (!activeLessons.Any())
            {
                return 0;
            }

            var activeLessonIds = activeLessons
                .Select(l => l.Id)
                .ToList();

            var completedLessons = await context.LessonProgresses
                .CountAsync(x =>
                    x.StudentId == studentId &&
                    x.IsCompleted == true &&
                    activeLessonIds.Contains(x.LessonId));

            return (int)Math.Round(((decimal)completedLessons / activeLessons.Count) * 100);
        }
        public async Task<StudentCourseContentModulesDto?> GetModulesAndLessonsAsync(int studentId, int courseId)
        {
            var course = await GetAllowedCourseAsync(studentId, courseId);

            if (course == null)
            {
                return null;
            }

            var visibleModules = await GetVisibleModulesWithQuizAsync(courseId);

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.StudentQuizAttempts
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var activeResets = await context.QuizAttemptsResets
                .Where(x => x.StudentId == studentId && x.LockedUntil > DateTime.UtcNow)
                .ToListAsync();

            var moduleDtos = new List<StudentCourseContentModuleDto>();

            foreach (var module in visibleModules)
            {
                var activeLessons = module.Lessons?
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList() ?? new List<Lesson>();

                var completedLessons = activeLessons.Count(lesson =>
                    lessonProgresses.Any(progress =>
                        progress.LessonId == lesson.Id &&
                        progress.IsCompleted));

                var totalLessons = activeLessons.Count;

                var lessonProgressPercentage = totalLessons > 0
                    ? (int)Math.Round(((decimal)completedLessons / totalLessons) * 100)
                    : 0;

                var quizDtos = new List<StudentCourseContentModuleQuizDto>();

                if (module.Quiz != null && module.Quiz.IsActive && module.Quiz.IsAccessible)
                {
                    var isPassed = quizAttempts.Any(x =>
                        x.QuizId == module.Quiz.Id &&
                        x.IsPassed);

                    var reset = activeResets.FirstOrDefault(x => x.QuizId == module.Quiz.Id);

                    quizDtos.Add(new StudentCourseContentModuleQuizDto
                    {
                        Id = module.Quiz.Id,
                        Title = module.Quiz.Title,
                        Instructions = module.Quiz.Instructions,
                        AttemptsAllowed = 3,
                        QuestionsCount = module.Quiz.Questions?.Count ?? 0,
                        IsPassed = isPassed,
                        IsLocked = reset != null,
                        LockedUntil = reset?.LockedUntil
                    });
                }

                var isCompleted = IsModuleCompleted(module, lessonProgresses, quizAttempts);

                moduleDtos.Add(new StudentCourseContentModuleDto
                {
                    Id = module.Id,
                    Title = module.Title,
                    Description = module.Description,
                    DisplayOrder = module.DisplayOrder,
                    IsAccessible = false,
                    IsCompleted = isCompleted,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    ProgressPercentage = lessonProgressPercentage,
                    StatusText = GetModuleStatusText(totalLessons, completedLessons, isCompleted),
                    Lessons = activeLessons.Select(lesson => new StudentCourseContentLessonDto
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        DisplayOrder = lesson.DisplayOrder,
                        IsCompleted = lessonProgresses.Any(progress =>
                            progress.LessonId == lesson.Id &&
                            progress.IsCompleted)
                    }).ToList(),
                    Quizzes = quizDtos
                });
            }

            ApplyModuleAccess(moduleDtos);

            var progress = await CalculateCourseProgressAsync(studentId, courseId);

            return new StudentCourseContentModulesDto
            {
                CourseId = courseId,
                ProgressPercentage = progress,
                Modules = moduleDtos
            };
        }

        public async Task<bool> MarkLessonDoneAsync(int studentId, int lessonId)
        {
            var lesson = await context.Lessons
                .Include(x => x.Module)
                .FirstOrDefaultAsync(x =>
                    x.Id == lessonId &&
                    x.IsActive);

            if (lesson == null || lesson.Module == null)
            {
                return false;
            }

            var isEnrolled = await context.CourseEnrollments
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.CourseId == lesson.Module.CourseId &&
                    x.Status == "Active");

            if (!isEnrolled)
            {
                return false;
            }

            var progress = await context.LessonProgresses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsViewed = true,
                    ViewedAt = DateTime.UtcNow,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsViewed = true;
                progress.ViewedAt ??= DateTime.UtcNow;
                progress.IsCompleted = true;
                progress.CompletedAt ??= DateTime.UtcNow;

                context.LessonProgresses.Update(progress);
            }

            await UpdateModuleProgressIfCompletedAsync(studentId, lesson.ModuleId);

            return await context.SaveChangesAsync() >= 0;
        }
    
    private async Task<List<Module>> GetVisibleModulesWithQuizAsync(int courseId)
        {
            var modules = await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz!)
                    .ThenInclude(q => q.Questions)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.IsActive == true)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return modules
                .Where(x => x.Lessons != null && x.Lessons.Any(l => l.IsActive))
                .ToList();
        }

        private static bool IsModuleCompleted(
            Module module,
            List<LessonProgress> lessonProgresses,
            List<StudentQuizAttempt> quizAttempts)
        {
            var activeLessons = module.Lessons?
                .Where(x => x.IsActive)
                .ToList() ?? new List<Lesson>();

            if (!activeLessons.Any())
            {
                return false;
            }

            var allLessonsCompleted = activeLessons.All(lesson =>
                lessonProgresses.Any(progress =>
                    progress.LessonId == lesson.Id &&
                    progress.IsCompleted));

            if (!allLessonsCompleted)
            {
                return false;
            }

            if (module.Quiz == null || !module.Quiz.IsActive || !module.Quiz.IsAccessible)
            {
                return true;
            }

            return quizAttempts.Any(attempt =>
                attempt.QuizId == module.Quiz.Id &&
                attempt.IsPassed);
        }

        private static void ApplyModuleAccess(List<StudentCourseContentModuleDto> modules)
        {
            if (!modules.Any())
            {
                return;
            }

            modules[0].IsAccessible = true;

            for (var i = 1; i < modules.Count; i++)
            {
                modules[i].IsAccessible = modules[i - 1].IsCompleted;
            }
        }

        private static string GetModuleStatusText(int totalLessons, int completedLessons, bool isCompleted)
        {
            if (isCompleted)
            {
                return "Completed";
            }

            if (completedLessons <= 0)
            {
                return "Not Started";
            }

            if (completedLessons < totalLessons)
            {
                return "In Progress";
            }

            return "Quiz Pending";
        }

        private async Task UpdateModuleProgressIfCompletedAsync(int studentId, int moduleId)
        {
            var module = await context.Modules
                .Include(x => x.Lessons)
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.Id == moduleId);

            if (module == null)
            {
                return;
            }

            var lessonProgresses = await context.LessonProgresses
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var quizAttempts = await context.StudentQuizAttempts
                .Where(x => x.StudentId == studentId)
                .ToListAsync();

            var isCompleted = IsModuleCompleted(module, lessonProgresses, quizAttempts);

            if (!isCompleted)
            {
                return;
            }

            var moduleProgress = await context.ModuleProgresses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.ModuleId == moduleId);

            if (moduleProgress == null)
            {
                moduleProgress = new ModuleProgress
                {
                    StudentId = studentId,
                    ModuleId = moduleId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                context.ModuleProgresses.Add(moduleProgress);
            }
            else
            {
                moduleProgress.IsCompleted = true;
                moduleProgress.CompletedAt ??= DateTime.UtcNow;

                context.ModuleProgresses.Update(moduleProgress);
            }
        }
    }
}
    