using HTFLMS.Data;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos.TrainerAssignmentGrading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HTFLMS.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Trainer")]
    public class TrainerAssignmentGradingController
        : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMailService mailService;
        private readonly ApplicationDbContext context;
        private readonly ILogger<TrainerAssignmentGradingController> logger;

        public TrainerAssignmentGradingController(
            IUnitOfWork uow,
            IMailService mailService,
            ApplicationDbContext context,
            ILogger<TrainerAssignmentGradingController> logger)
        {
            this.uow = uow;
            this.mailService = mailService;
            this.context = context;
            this.logger = logger;
        }

        [HttpGet("submissions")]
        public async Task<IActionResult> GetSubmissions(
            [FromQuery] string? search,
            [FromQuery] int? courseId,
            [FromQuery] int? moduleId,
            [FromQuery] string? status)
        {
            var trainerId =
                GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result =
                await uow.TrainerAssignmentGradingService
                    .GetSubmissionsAsync(
                        trainerId,
                        search,
                        courseId,
                        moduleId,
                        status);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("submission/{submissionId:int}")]
        public async Task<IActionResult> GetSubmissionDetail(
            int submissionId)
        {
            var trainerId =
                GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result =
                await uow.TrainerAssignmentGradingService
                    .GetSubmissionDetailAsync(
                        trainerId,
                        submissionId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Submission not found or you are not allowed to access it."
                });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost("submission/{submissionId:int}/grade")]
        public async Task<IActionResult> SaveGrade(
            int submissionId,
            [FromBody] TrainerAssignmentGradingSaveDto dto)
        {
            var trainerId =
                GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid grade data."
                });
            }

            try
            {
                var beforeGrade =
                    await uow.TrainerAssignmentGradingService
                        .GetSubmissionDetailAsync(
                            trainerId,
                            submissionId);

                if (beforeGrade == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Submission not found or you are not allowed to grade it."
                    });
                }

                var wasAlreadyGraded =
                    beforeGrade.IsGraded;

                var result =
                    await uow.TrainerAssignmentGradingService
                        .SaveGradeAsync(
                            trainerId,
                            submissionId,
                            dto);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Submission not found or you are not allowed to grade it."
                    });
                }

                await SendGradeNotificationAsync(
                    beforeGrade.StudentId,
                    beforeGrade.StudentName,
                    beforeGrade.CourseTitle,
                    beforeGrade.AssignmentTitle,
                    result.ScoreText,
                    wasAlreadyGraded);

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unable to save assignment grade. SubmissionId: {SubmissionId}",
                    submissionId);

                return BadRequest(new
                {
                    success = false,
                    message =
                        "Unable to save grade. Please try again."
                });
            }
        }

        [HttpPost("missing/mark-zero")]
        public async Task<IActionResult> MarkMissingSubmissionZero(
            [FromBody] TrainerAssignmentGradingMarkZeroDto dto)
        {
            var trainerId =
                GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message =
                        "Invalid missing submission data."
                });
            }

            try
            {
                var assignment =
                    await context.Assignments
                        .Include(x => x.Course)
                        .FirstOrDefaultAsync(x =>
                            x.Id == dto.AssignmentId &&
                            x.Course != null &&
                            x.Course.TrainerId == trainerId);

                var student =
                    await context.Users
                        .FirstOrDefaultAsync(x =>
                            x.Id == dto.StudentId);

                var result =
                    await uow.TrainerAssignmentGradingService
                        .MarkMissingSubmissionZeroAsync(
                            trainerId,
                            dto);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Assignment/student record not found or you are not allowed to update it."
                    });
                }

                if (assignment != null &&
                    assignment.Course != null &&
                    student != null)
                {
                    await SendGradeNotificationAsync(
                        student.Id,
                        student.Name,
                        assignment.Course.Title,
                        assignment.Title,
                        result.ScoreText,
                        false);
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unable to mark missing assignment as zero. AssignmentId: {AssignmentId}, StudentId: {StudentId}",
                    dto.AssignmentId,
                    dto.StudentId);

                return BadRequest(new
                {
                    success = false,
                    message =
                        "Unable to mark missing submission as 0. Please try again."
                });
            }
        }

        private async Task SendGradeNotificationAsync(
            int studentId,
            string studentName,
            string courseTitle,
            string assignmentTitle,
            string scoreText,
            bool wasAlreadyGraded)
        {
            try
            {
                var notificationTitle =
                    wasAlreadyGraded
                        ? "Assignment Grade Updated"
                        : "Assignment Graded";

                var notificationMessage =
                    wasAlreadyGraded
                        ? $"Your grade for \"{assignmentTitle}\" has been updated to {scoreText}."
                        : $"Your assignment \"{assignmentTitle}\" has been graded. Score: {scoreText}.";

                await uow.NotificationService
                    .CreateAsync(
                        studentId,
                        notificationTitle,
                        notificationMessage,
                        "Assignment",
                        "/Student/Grades");

                if (wasAlreadyGraded)
                    return;

                var student =
                    await context.Users
                        .FirstOrDefaultAsync(x =>
                            x.Id == studentId);

                if (student == null ||
                    string.IsNullOrWhiteSpace(student.Email))
                {
                    return;
                }

                try
                {
                    await mailService
                        .SendAssignmentGradedEmailAsync(
                            student.Email,
                            studentName,
                            courseTitle,
                            assignmentTitle,
                            scoreText);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Assignment graded email failed. StudentId: {StudentId}",
                        studentId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Assignment grade notification failed. StudentId: {StudentId}",
                    studentId);
            }
        }

        private int GetLoggedInTrainerId()
        {
            var trainerIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(
                trainerIdClaim))
            {
                return 0;
            }

            return int.TryParse(
                trainerIdClaim,
                out var trainerId)
                    ? trainerId
                    : 0;
        }
    }
}