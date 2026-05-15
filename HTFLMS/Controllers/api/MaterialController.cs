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
    public class MaterialController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IWebHostEnvironment env;

        public MaterialController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            this.uow = uow;
            this.env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] MaterialDto dto)
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
                return Unauthorized(new APIError(401, "You are not allowed to add material to this course."));

            if (dto.ModuleId.HasValue)
            {
                var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId.Value);

                if (module == null)
                    return NotFound(new APIError(404, "Module not found."));

                if (module.CourseId != dto.CourseId)
                    return BadRequest(new APIError(400, "Selected module does not belong to this course."));
            }

            string? savedFilePath = null;

            if (dto.ContentType != "Video Link")
            {
                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new APIError(400, "Please upload a file."));

                savedFilePath = await FileUploadHelper.SaveFileAsync(
                    dto.File,
                    "uploads/materials",
                    env
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.ExternalUrl))
                    return BadRequest(new APIError(400, "Please provide a video link."));
            }

            var material = new Material
            {
                CourseId = dto.CourseId,
                ModuleId = dto.ModuleId,
                LessonId = dto.LessonId,
                Title = dto.Title,
                ContentType = dto.ContentType,
                FilePath = savedFilePath,
                ExternalUrl = dto.ExternalUrl,
                Pages = dto.Pages,
                Slides = dto.Slides,
                Minutes = dto.Minutes,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            uow.MaterialService.Add(material);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Material created successfully.",
                materialId = material.Id
            });
        }

        [HttpGet("course/{courseId:int}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var materials = await uow.MaterialService.GetByCourseIdAsync(courseId);

            var result = materials.Select(m => new
            {
                m.Id,
                m.CourseId,
                m.ModuleId,
                ModuleTitle = m.Module != null ? m.Module.Title : null,
                m.LessonId,
                m.Title,
                m.ContentType,
                m.FilePath,
                m.ExternalUrl,
                m.Pages,
                m.Slides,
                m.Minutes,
                m.IsActive,
                m.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("module/{moduleId:int}")]
        public async Task<IActionResult> GetByModule(int moduleId)
        {
            var materials = await uow.MaterialService.GetByModuleIdAsync(moduleId);

            var result = materials.Select(m => new
            {
                m.Id,
                m.CourseId,
                m.ModuleId,
                ModuleTitle = m.Module != null ? m.Module.Title : null,
                m.LessonId,
                m.Title,
                m.ContentType,
                m.FilePath,
                m.ExternalUrl,
                m.Pages,
                m.Slides,
                m.Minutes,
                m.IsActive,
                m.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await uow.MaterialService.GetByIdAsync(id);

            if (material == null)
                return NotFound(new APIError(404, "Material not found."));

            return Ok(new
            {
                material.Id,
                material.CourseId,
                material.ModuleId,
                ModuleTitle = material.Module != null ? material.Module.Title : null,
                material.LessonId,
                material.Title,
                material.ContentType,
                material.FilePath,
                material.ExternalUrl,
                material.Pages,
                material.Slides,
                material.Minutes,
                material.IsActive,
                material.CreatedAt
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromForm] MaterialDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var material = await uow.MaterialService.GetByIdAsync(id);

            if (material == null)
                return NotFound(new APIError(404, "Material not found."));

            material.CourseId = dto.CourseId;
            material.ModuleId = dto.ModuleId;
            material.LessonId = dto.LessonId;
            material.Title = dto.Title;
            material.ContentType = dto.ContentType;
            material.ExternalUrl = dto.ExternalUrl;
            material.Pages = dto.Pages;
            material.Slides = dto.Slides;
            material.Minutes = dto.Minutes;
            material.IsActive = dto.IsActive;

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.File,
                material.FilePath,
                "uploads/materials",
                env,
                path => material.FilePath = path
            );

            uow.MaterialService.Update(material);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Material updated successfully.",
                materialId = material.Id
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await uow.MaterialService.GetByIdAsync(id);

            if (material == null)
                return NotFound(new APIError(404, "Material not found."));

            uow.MaterialService.Delete(material);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Material deleted successfully."
            });
        }
    }
}