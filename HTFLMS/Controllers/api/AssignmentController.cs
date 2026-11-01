using HTFLMS.Data;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Helpers;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Controllers.api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IWebHostEnvironment env;
        private readonly IMailService mailService;
        private readonly ApplicationDbContext context;
        private readonly ILogger<AssignmentController> logger;

        public AssignmentController(
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IMailService mailService,
            ApplicationDbContext context,
            ILogger<AssignmentController> logger)
        {
            this.uow = uow;
            this.env = env;
            this.mailService = mailService;
            this.context = context;
            this.logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromForm] AssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "User is not logged in."));
            }

            var trainer =
                await uow.UserService
                    .GetUserByEmailAsync(email);

            if (trainer == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Logged-in trainer was not found."));
            }

            var course =
                await uow.CourseService
                    .GetByIdAsync(dto.CourseId);

            if (course == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Course not found."));
            }

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "You are not allowed to add assignment to this course."));
            }

            Module? module = null;

            if (dto.ModuleId.HasValue)
            {
                module =
                    await uow.ModuleService
                        .GetByIdAsync(dto.ModuleId.Value);

                if (module == null)
                {
                    return NotFound(
                        new APIError(
                            404,
                            "Module not found."));
                }

                if (module.CourseId != dto.CourseId)
                {
                    return BadRequest(
                        new APIError(
                            400,
                            "Selected module does not belong to this course."));
                }
            }

            string? savedFilePath = null;

            if (dto.File != null &&
                dto.File.Length > 0)
            {
                savedFilePath =
                    await FileUploadHelper.SaveFileAsync(
                        dto.File,
                        "uploads/assignments",
                        env);
            }

            var assignment = new Assignment
            {
                CourseId = dto.CourseId,
                ModuleId = dto.ModuleId,
                Title = dto.Title,
                Description = dto.Description,
                Marks = dto.Marks,
                DueDateTime = dto.DueDateTime,
                FilePath = savedFilePath,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            uow.AssignmentService.Add(assignment);

            var saved = await uow.SaveAsync();

            if (!saved)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Assignment could not be created."));
            }

            var shouldNotify =
                course.IsActive &&
                course.IsPublished &&
                assignment.IsActive;

            if (module != null)
            {
                shouldNotify =
                    shouldNotify &&
                    module.IsActive &&
                    module.IsAccessible;
            }

            if (shouldNotify)
            {
                await NotifyStudentsAboutAssignmentAsync(
                    assignment,
                    course);
            }

            return Ok(new
            {
                message = "Assignment created successfully.",
                assignmentId = assignment.Id
            });
        }

        [HttpGet("course/{courseId:int}")]
        public async Task<IActionResult> GetByCourse(
            int courseId)
        {
            var assignments =
                await uow.AssignmentService
                    .GetByCourseIdAsync(courseId);

            var result = assignments.Select(a => new
            {
                a.Id,
                a.CourseId,
                a.ModuleId,

                ModuleTitle =
                    a.Module != null
                        ? a.Module.Title
                        : null,

                a.Title,

                Instructions =
                    a.Description,

                a.Description,
                a.Marks,
                a.DueDateTime,
                a.FilePath,
                a.IsActive,
                a.CreatedAt,

                SubmittedCount =
                    a.Submissions != null
                        ? a.Submissions.Count
                        : 0
            });

            return Ok(result);
        }

        [HttpGet("module/{moduleId:int}")]
        public async Task<IActionResult> GetByModule(
            int moduleId)
        {
            var assignments =
                await uow.AssignmentService
                    .GetByModuleIdAsync(moduleId);

            var result = assignments.Select(a => new
            {
                a.Id,
                a.CourseId,
                a.ModuleId,

                ModuleTitle =
                    a.Module != null
                        ? a.Module.Title
                        : null,

                a.Title,

                Instructions =
                    a.Description,

                a.Description,
                a.Marks,
                a.DueDateTime,
                a.FilePath,
                a.IsActive,
                a.CreatedAt,

                SubmittedCount =
                    a.Submissions != null
                        ? a.Submissions.Count
                        : 0
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var assignment =
                await uow.AssignmentService
                    .GetByIdAsync(id);

            if (assignment == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Assignment not found."));
            }

            return Ok(new
            {
                assignment.Id,
                assignment.CourseId,
                assignment.ModuleId,

                ModuleTitle =
                    assignment.Module != null
                        ? assignment.Module.Title
                        : null,

                assignment.Title,

                Instructions =
                    assignment.Description,

                assignment.Description,
                assignment.Marks,
                assignment.DueDateTime,
                assignment.FilePath,
                assignment.IsActive,
                assignment.CreatedAt,

                SubmittedCount =
                    assignment.Submissions != null
                        ? assignment.Submissions.Count
                        : 0
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id,
            [FromForm] AssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "User is not logged in."));
            }

            var trainer =
                await uow.UserService
                    .GetUserByEmailAsync(email);

            if (trainer == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Logged-in trainer was not found."));
            }

            var assignment =
                await uow.AssignmentService
                    .GetByIdAsync(id);

            if (assignment == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Assignment not found."));
            }

            var course =
                await uow.CourseService
                    .GetByIdAsync(assignment.CourseId);

            if (course == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Course not found."));
            }

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "You are not allowed to edit this assignment."));
            }

            if (dto.CourseId != assignment.CourseId)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Assignment course cannot be changed."));
            }

            if (dto.ModuleId.HasValue)
            {
                var module =
                    await uow.ModuleService
                        .GetByIdAsync(dto.ModuleId.Value);

                if (module == null)
                {
                    return NotFound(
                        new APIError(
                            404,
                            "Module not found."));
                }

                if (module.CourseId != assignment.CourseId)
                {
                    return BadRequest(
                        new APIError(
                            400,
                            "Selected module does not belong to this course."));
                }
            }

            assignment.ModuleId = dto.ModuleId;
            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.Marks = dto.Marks;
            assignment.DueDateTime = dto.DueDateTime;
            assignment.IsActive = dto.IsActive;

            await FileUploadHelper
                .ReplaceFileIfUploadedAsync(
                    dto.File,
                    assignment.FilePath,
                    "uploads/assignments",
                    env,
                    path =>
                        assignment.FilePath = path);

            uow.AssignmentService.Update(assignment);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Assignment updated successfully.",
                assignmentId = assignment.Id
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "User is not logged in."));
            }

            var trainer =
                await uow.UserService
                    .GetUserByEmailAsync(email);

            if (trainer == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Logged-in trainer was not found."));
            }

            var assignment =
                await uow.AssignmentService
                    .GetByIdAsync(id);

            if (assignment == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Assignment not found."));
            }

            var course =
                await uow.CourseService
                    .GetByIdAsync(assignment.CourseId);

            if (course == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Course not found."));
            }

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "You are not allowed to delete this assignment."));
            }

            uow.AssignmentService.Delete(assignment);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Assignment deleted successfully."
            });
        }

        private async Task NotifyStudentsAboutAssignmentAsync(
            Assignment assignment,
            Course course)
        {
            try
            {
                var studentIds =
                    await uow.CourseEnrollmentService
                        .GetActiveStudentIdsByCourseAsync(
                            course.Id);

                if (studentIds.Count == 0)
                    return;

                await uow.NotificationService.CreateBulkAsync(
                    studentIds,
                    "New Assignment Available",
                    $"A new assignment \"{assignment.Title}\" has been added to {course.Title}.",
                    "Assignment",
                    "/Student/Courses");

                var students =
                    await context.Users
                        .Where(x =>
                            studentIds.Contains(x.Id) &&
                            x.IsActive)
                        .ToListAsync();

                foreach (var student in students)
                {
                    if (string.IsNullOrWhiteSpace(student.Email))
                        continue;

                    try
                    {
                        await mailService
                            .SendNewAssignmentEmailAsync(
                                student.Email,
                                student.Name,
                                course.Title,
                                assignment.Title,
                                assignment.DueDateTime);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "New assignment email failed. StudentId: {StudentId}, AssignmentId: {AssignmentId}",
                            student.Id,
                            assignment.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "New assignment notification failed. AssignmentId: {AssignmentId}, CourseId: {CourseId}",
                    assignment.Id,
                    course.Id);
            }
        }
    }
}