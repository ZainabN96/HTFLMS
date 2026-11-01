using HTFLMS.Data.IServices;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class AssignmentDueReminderBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly ILogger<AssignmentDueReminderBackgroundService>
            logger;

        private static readonly TimeSpan CheckInterval =
            TimeSpan.FromMinutes(15);

        private static readonly TimeSpan ReminderWindow =
            TimeSpan.FromHours(24);

        public AssignmentDueReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AssignmentDueReminderBackgroundService> logger)
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
                    await ProcessDueRemindersAsync(
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Assignment due reminder background process failed.");
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


        private async Task ProcessDueRemindersAsync(
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

            var mailService =
                scope.ServiceProvider
                    .GetRequiredService<IMailService>();

            var now = DateTime.Now;

            var reminderEnd =
                now.Add(ReminderWindow);


            // =========================================
            // FIND ASSIGNMENTS DUE WITHIN 24 HOURS
            // =========================================

            var assignments =
                await context.Assignments
                    .Include(x => x.Course)
                    .Include(x => x.Module)
                    .Where(x =>
                        x.IsActive &&
                        x.Course != null &&
                        x.Course.IsActive &&
                        x.Course.IsPublished &&
                        x.DueDateTime > now &&
                        x.DueDateTime <= reminderEnd)
                    .ToListAsync(cancellationToken);


            if (assignments.Count == 0)
                return;


            foreach (var assignment in assignments)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (assignment.Course == null)
                    continue;


                // If assignment belongs to a module,
                // inactive module assignment should not
                // generate a reminder.
                if (assignment.ModuleId.HasValue &&
                    (assignment.Module == null ||
                     !assignment.Module.IsActive))
                {
                    continue;
                }


                // =========================================
                // ACTIVE ENROLLED STUDENTS
                // =========================================

                var students =
                    await (
                        from enrollment in context.CourseEnrollments

                        join student in context.Users
                            on enrollment.StudentId equals student.Id

                        where
                            enrollment.CourseId == assignment.CourseId &&
                            enrollment.Status == "Active" &&
                            student.IsActive

                        select student
                    )
                    .ToListAsync(cancellationToken);


                foreach (var student in students)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;


                    // =========================================
                    // ALREADY SUBMITTED?
                    // =========================================

                    var hasSubmitted =
                        await context.AssignmentSubmissions
                            .AnyAsync(
                                x =>
                                    x.AssignmentId == assignment.Id &&
                                    x.StudentId == student.Id,
                                cancellationToken);


                    if (hasSubmitted)
                        continue;


                    // =========================================
                    // GET / CREATE TRACKER
                    // =========================================

                    var reminder =
                        await context.AssignmentDueReminders
                            .FirstOrDefaultAsync(
                                x =>
                                    x.AssignmentId == assignment.Id &&
                                    x.StudentId == student.Id,
                                cancellationToken);


                    if (reminder == null)
                    {
                        reminder =
                            new AssignmentDueReminder
                            {
                                AssignmentId = assignment.Id,

                                StudentId = student.Id,

                                NotificationSent = false,

                                EmailSent = false,

                                CreatedAt = DateTime.UtcNow
                            };


                        context.AssignmentDueReminders.Add(
                            reminder);


                        try
                        {
                            await context.SaveChangesAsync(
                                cancellationToken);
                        }
                        catch (DbUpdateException ex)
                        {
                            logger.LogWarning(
                                ex,
                                "Assignment reminder tracker already exists or could not be created. AssignmentId: {AssignmentId}, StudentId: {StudentId}",
                                assignment.Id,
                                student.Id);


                            context.Entry(reminder)
                                .State =
                                EntityState.Detached;


                            reminder =
                                await context.AssignmentDueReminders
                                    .FirstOrDefaultAsync(
                                        x =>
                                            x.AssignmentId ==
                                            assignment.Id &&
                                            x.StudentId ==
                                            student.Id,
                                        cancellationToken);


                            if (reminder == null)
                                continue;
                        }
                    }


                    // =========================================
                    // IN-APP + SIGNALR NOTIFICATION
                    // =========================================

                    if (!reminder.NotificationSent)
                    {
                        try
                        {
                            await uow.NotificationService
                                .CreateAsync(
                                    student.Id,
                                    "Assignment Due Reminder",
                                    $"Your assignment \"{assignment.Title}\" for {assignment.Course.Title} is due within 24 hours.",
                                    "AssignmentDueReminder",
                                    "/Student/Courses");


                            reminder.NotificationSent = true;

                            reminder.NotificationSentAt =
                                DateTime.UtcNow;


                            await context.SaveChangesAsync(
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Assignment due notification failed. AssignmentId: {AssignmentId}, StudentId: {StudentId}",
                                assignment.Id,
                                student.Id);
                        }
                    }


                    // =========================================
                    // EMAIL
                    // =========================================

                    if (!reminder.EmailSent &&
                        !string.IsNullOrWhiteSpace(student.Email))
                    {
                        try
                        {
                            await mailService
                                .SendAssignmentDueReminderEmailAsync(
                                    student.Email,
                                    student.Name,
                                    assignment.Course.Title,
                                    assignment.Title,
                                    assignment.DueDateTime);


                            reminder.EmailSent = true;

                            reminder.EmailSentAt =
                                DateTime.UtcNow;


                            await context.SaveChangesAsync(
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Assignment due reminder email failed. AssignmentId: {AssignmentId}, StudentId: {StudentId}",
                                assignment.Id,
                                student.Id);
                        }
                    }
                }
            }
        }
    }
}