using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Helpers;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IWebHostEnvironment env;

        public AssignmentController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            this.uow = uow;
            this.env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] AssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var course = await uow.CourseService.GetByIdAsync(dto.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (course.TrainerId != trainer.Id)
                return Unauthorized(new APIError(401, "You are not allowed to add assignment to this course."));

            if (dto.ModuleId.HasValue)
            {
                var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId.Value);

                if (module == null)
                    return NotFound(new APIError(404, "Module not found."));

                if (module.CourseId != dto.CourseId)
                    return BadRequest(new APIError(400, "Selected module does not belong to this course."));
            }

            string? savedFilePath = null;

            if (dto.File != null && dto.File.Length > 0)
            {
                savedFilePath = await FileUploadHelper.SaveFileAsync(
                    dto.File,
                    "uploads/assignments",
                    env
                );
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
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Assignment created successfully.",
                assignmentId = assignment.Id
            });
        }

        [HttpGet("course/{courseId:int}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var assignments = await uow.AssignmentService.GetByCourseIdAsync(courseId);

            var result = assignments.Select(a => new
            {
                a.Id,
                a.CourseId,
                a.ModuleId,
                ModuleTitle = a.Module != null ? a.Module.Title : null,
                a.Title,
                Instructions = a.Description,
                a.Description,
                a.Marks,
                a.DueDateTime,
                a.FilePath,
                a.IsActive,
                a.CreatedAt,
                SubmittedCount = a.Submissions != null ? a.Submissions.Count : 0
            });

            return Ok(result);
        }

        [HttpGet("module/{moduleId:int}")]
        public async Task<IActionResult> GetByModule(int moduleId)
        {
            var assignments = await uow.AssignmentService.GetByModuleIdAsync(moduleId);

            var result = assignments.Select(a => new
            {
                a.Id,
                a.CourseId,
                a.ModuleId,
                ModuleTitle = a.Module != null ? a.Module.Title : null,
                a.Title,
                Instructions = a.Description,
                a.Description,
                a.Marks,
                a.DueDateTime,
                a.FilePath,
                a.IsActive,
                a.CreatedAt,
                SubmittedCount = a.Submissions != null ? a.Submissions.Count : 0
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await uow.AssignmentService.GetByIdAsync(id);

            if (assignment == null)
                return NotFound(new APIError(404, "Assignment not found."));

            return Ok(new
            {
                assignment.Id,
                assignment.CourseId,
                assignment.ModuleId,
                ModuleTitle = assignment.Module != null ? assignment.Module.Title : null,
                assignment.Title,
                Instructions = assignment.Description,
                assignment.Description,
                assignment.Marks,
                assignment.DueDateTime,
                assignment.FilePath,
                assignment.IsActive,
                assignment.CreatedAt,
                SubmittedCount = assignment.Submissions != null ? assignment.Submissions.Count : 0
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromForm] AssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var assignment = await uow.AssignmentService.GetByIdAsync(id);

            if (assignment == null)
                return NotFound(new APIError(404, "Assignment not found."));

            assignment.CourseId = dto.CourseId;
            assignment.ModuleId = dto.ModuleId;
            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.Marks = dto.Marks;
            assignment.DueDateTime = dto.DueDateTime;
            assignment.IsActive = dto.IsActive;

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.File,
                assignment.FilePath,
                "uploads/assignments",
                env,
                path => assignment.FilePath = path
            );

            uow.AssignmentService.Update(assignment);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Assignment updated successfully.",
                assignmentId = assignment.Id
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await uow.AssignmentService.GetByIdAsync(id);

            if (assignment == null)
                return NotFound(new APIError(404, "Assignment not found."));

            uow.AssignmentService.Delete(assignment);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Assignment deleted successfully."
            });
        }
    }
}