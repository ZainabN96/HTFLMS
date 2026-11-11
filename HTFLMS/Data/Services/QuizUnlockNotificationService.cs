using HTFLMS.Data.IServices;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class QuizUnlockNotificationService
        : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly ILogger<QuizUnlockNotificationService>
            logger;

        private static readonly TimeSpan CheckInterval =
            TimeSpan.FromMinutes(1);

        public QuizUnlockNotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<QuizUnlockNotificationService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessUnlockedQuizzesAsync(
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Quiz unlock notification process failed.");
                }

                try
                {
                    await Task.Delay(
                        CheckInterval,
                        stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessUnlockedQuizzesAsync(
            CancellationToken cancellationToken)
        {
            using var scope =
                scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var uow =
                scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

            var now =
                DateTime.UtcNow;

            var expiredResets =
                await context.QuizAttemptsResets
                    .Include(x => x.Student)
                    .Include(x => x.Quiz)
                        .ThenInclude(x => x!.Module)
                    .Where(x =>
                        x.LockedUntil <= now &&
                        x.UnlockNotificationSentAt == null)
                    .OrderBy(x => x.LockedUntil)
                    .ToListAsync(cancellationToken);

            foreach (var reset in expiredResets)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (reset.Student == null ||
                    !reset.Student.IsActive)
                {
                    continue;
                }

                if (reset.Quiz == null ||
                    !reset.Quiz.IsActive ||
                    !reset.Quiz.IsAccessible)
                {
                    continue;
                }

                if (reset.Quiz.Module == null ||
                    !reset.Quiz.Module.IsActive ||
                    !reset.Quiz.Module.IsAccessible)
                {
                    continue;
                }

                var course =
                    await context.Courses
                        .FirstOrDefaultAsync(
                            x =>
                                x.Id ==
                                reset.Quiz.Module.CourseId,
                            cancellationToken);

                if (course == null ||
                    !course.IsActive ||
                    !course.IsPublished)
                {
                    continue;
                }

                var isActiveEnrollment =
                    await context.CourseEnrollments
                        .AnyAsync(
                            x =>
                                x.StudentId ==
                                reset.StudentId &&
                                x.CourseId ==
                                course.Id &&
                                x.Status == "Active",
                            cancellationToken);

                if (!isActiveEnrollment)
                    continue;

                try
                {
                    await uow.NotificationService
                        .CreateAsync(
                            reset.StudentId,
                            "Quiz Unlocked",
                            $"Your quiz \"{reset.Quiz.Title}\" has been unlocked. Your attempts have been reset and you can try again.",
                            "Quiz",
                            "/Student/Courses");

                    reset.UnlockNotificationSentAt =
                        DateTime.UtcNow;

                    await context.SaveChangesAsync(
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Quiz unlock notification failed. ResetId: {ResetId}, StudentId: {StudentId}, QuizId: {QuizId}",
                        reset.Id,
                        reset.StudentId,
                        reset.QuizId);
                }
            }
        }
    }
}