using AutoMapper;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;

        public LessonController(IUnitOfWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] LessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to add lessons to this module.")
                );
            }

            var lesson = mapper.Map<Lesson>(dto);

            lesson.ModuleId = dto.ModuleId;
            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.DisplayOrder = dto.DisplayOrder;
            lesson.IsActive = dto.IsActive;
            lesson.CreatedAt = DateTime.UtcNow;

            uow.LessonService.Add(lesson);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Lesson created successfully.",
                lessonId = lesson.Id
            });
        }

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLessonsByModule(int moduleId)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var module = await uow.ModuleService.GetByIdAsync(moduleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to view these lessons.")
                );
            }

            var lessons = await uow.LessonService.GetByModuleIdAsync(moduleId);

            var result = lessons.Select(l => new
            {
                l.Id,
                l.ModuleId,
                l.Title,
                l.Description,
                l.DisplayOrder,
                l.IsActive,
                l.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLesson(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var lesson = await uow.LessonService.GetByIdAsync(id);

            if (lesson == null)
                return NotFound(new APIError(404, "Lesson not found."));

            var module = await uow.ModuleService.GetByIdAsync(lesson.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to view this lesson.")
                );
            }

            return Ok(new
            {
                lesson.Id,
                lesson.ModuleId,
                lesson.Title,
                lesson.Description,
                lesson.DisplayOrder,
                lesson.IsActive
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromForm] LessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var lesson = await uow.LessonService.GetByIdAsync(id);

            if (lesson == null)
                return NotFound(new APIError(404, "Lesson not found."));

            var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to edit this lesson.")
                );
            }

            lesson.ModuleId = dto.ModuleId;
            lesson.Title = dto.Title;
            lesson.Description = dto.Description;
            lesson.DisplayOrder = dto.DisplayOrder;
            lesson.IsActive = dto.IsActive;

            uow.LessonService.Update(lesson);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Lesson updated successfully.",
                lessonId = lesson.Id
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var lesson = await uow.LessonService.GetByIdAsync(id);

            if (lesson == null)
                return NotFound(new APIError(404, "Lesson not found."));

            var module = await uow.ModuleService.GetByIdAsync(lesson.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to delete this lesson.")
                );
            }

            uow.LessonService.Delete(lesson);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Lesson deleted successfully."
            });
        }
    }
}