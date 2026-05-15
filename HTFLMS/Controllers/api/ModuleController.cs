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
    public class ModuleController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;

        public ModuleController(IUnitOfWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ModuleDto dto)
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
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to add modules to this course.")
                );
            }

            var module = mapper.Map<Module>(dto);

            module.CourseId = dto.CourseId;
            module.Title = dto.Title;
            module.Description = dto.Description;
            module.DisplayOrder = dto.DisplayOrder;
            module.IsActive = dto.IsActive;
            module.IsAccessible = dto.IsAccessible;
            module.CreatedAt = DateTime.UtcNow;

            uow.ModuleService.Add(module);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Module created successfully.",
                moduleId = module.Id
            });
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetModulesByCourse(int courseId)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var course = await uow.CourseService.GetByIdAsync(courseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to view these modules.")
                );
            }

            var modules = await uow.ModuleService.GetByCourseIdAsync(courseId);

            var result = modules.Select(m => new
            {
                m.Id,
                m.CourseId,
                m.Title,
                m.Description,
                m.DisplayOrder,
                m.IsActive,
                m.IsAccessible,
                m.CreatedAt,

                LessonsCount = m.Lessons == null
                    ? 0
                    : m.Lessons.Count,

                QuizCount = m.Quiz == null
                    ? 0
                    : 1
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetModule(int id)
        {
            var module = await uow.ModuleService.GetByIdAsync(id);

            if (module == null)
            {
                return NotFound(
                    new APIError(404, "Module not found.")
                );
            }

            return Ok(new
            {
                module.Id,
                module.CourseId,
                module.Title,
                module.Description,
                module.DisplayOrder,
                module.IsActive,
                module.IsAccessible
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id,
            [FromForm] ModuleDto dto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var module = await uow.ModuleService.GetByIdAsync(id);

            if (module == null)
            {
                return NotFound(
                    new APIError(404, "Module not found.")
                );
            }

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to edit this module.")
                );
            }

            module.Title = dto.Title;
            module.Description = dto.Description;
            module.DisplayOrder = dto.DisplayOrder;
            module.IsActive = dto.IsActive;
            module.IsAccessible = dto.IsAccessible;

            uow.ModuleService.Update(module);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Module updated successfully.",
                moduleId = module.Id
            });
        }

        [HttpPut("toggle-access/{id:int}")]
        public async Task<IActionResult> ToggleAccess(
            int id,
            [FromBody] ToggleModuleAccessDto dto
        )
        {
            var module = await uow.ModuleService.GetByIdAsync(id);

            if (module == null)
            {
                return NotFound(
                    new APIError(404, "Module not found.")
                );
            }

            module.IsAccessible = dto.IsAccessible;

            uow.ModuleService.Update(module);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Module access updated successfully."
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var module = await uow.ModuleService.GetByIdAsync(id);

            if (module == null)
            {
                return NotFound(
                    new APIError(404, "Module not found.")
                );
            }

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (course.TrainerId != trainer.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to delete this module.")
                );
            }

            uow.ModuleService.Delete(module);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Module deleted successfully."
            });
        }
    }
}