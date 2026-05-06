using AutoMapper;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment env;

        public CourseController(IUnitOfWork uow, IMapper mapper, IWebHostEnvironment env)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = mapper.Map<Course>(dto);

            course.BatchNumber = dto.BatchNumber;
            course.CertificateIncluded = dto.CertificateIncluded;
            course.CreatedAt = DateTime.UtcNow;

            if (dto.Status == "Active")
            {
                course.IsActive = true;
                course.IsPublished = true;
            }
            else
            {
                course.IsActive = true;
                course.IsPublished = false;
            }

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                course.CourseImagePath = await SaveFile(dto.ImageFile, "uploads/courses/images");
            }

            if (dto.HandbookFile != null && dto.HandbookFile.Length > 0)
            {
                course.HandbookFilePath = await SaveFile(dto.HandbookFile, "uploads/courses/handbooks");
            }

            uow.CourseService.Add(course);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Course created successfully.",
                courseId = course.Id
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var courses = await uow.CourseService.GetByTrainerIdAsync(trainer.Id);

            var result = courses.Select(c => new
            {
                c.Id,
                c.Title,
                c.Category,
                c.Description,
                c.HandbookFilePath,
                c.CourseImagePath,
                c.TrainerId,
                c.IsPublished,
                c.IsActive,
                c.BatchStartDate,
                c.BatchNumber,
                c.BatchEndDate,
                c.CertificateIncluded,
                c.CreatedAt,
                TotalStudents = c.Enrollments == null ? 0 : c.Enrollments.Count
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await uow.CourseService.GetByIdAsync(id);

            if (course == null)
            {
                return NotFound(new APIError(
                    NotFound().StatusCode,
                    "Course not found."
                ));
            }

            return Ok(course);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] CourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = await uow.CourseService.GetByIdAsync(id);

            if (course == null)
            {
                return NotFound(new APIError(
                    NotFound().StatusCode,
                    "Course not found."
                ));
            }

            course.Title = dto.Title;
            course.Category = dto.Category;
            course.Description = dto.Description;
            course.TrainerId = dto.TrainerId;
            course.BatchStartDate = dto.BatchStartDate;
            course.BatchEndDate = dto.BatchEndDate;
            course.BatchNumber = dto.BatchNumber;
            course.CertificateIncluded = dto.CertificateIncluded;

            if (dto.Status == "Active")
            {
                course.IsActive = true;
                course.IsPublished = true;
            }
            else
            {
                course.IsActive = true;
                course.IsPublished = false;
            }

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                DeleteOldFile(course.CourseImagePath);
                course.CourseImagePath = await SaveFile(dto.ImageFile, "uploads/courses/images");
            }

            if (dto.HandbookFile != null && dto.HandbookFile.Length > 0)
            {
                DeleteOldFile(course.HandbookFilePath);
                course.HandbookFilePath = await SaveFile(dto.HandbookFile, "uploads/courses/handbooks");
            }

            uow.CourseService.Update(course);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Course updated successfully.",
                courseId = course.Id
            });
        }

        private async Task<string> SaveFile(IFormFile file, string folderPath)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, folderPath);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/" + folderPath.Replace("\\", "/") + "/" + fileName;
        }

        private void DeleteOldFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var cleanPath = filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(env.WebRootPath, cleanPath);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var trainer = await uow.UserService.GetUserByEmailAsync(email);

            if (trainer == null)
                return NotFound(new APIError(404, "Logged-in trainer was not found."));

            var course = await uow.CourseService.GetByIdAsync(id);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (course.TrainerId != trainer.Id)
                return Unauthorized(new APIError(401, "You are not allowed to delete this course."));

            DeleteOldFile(course.CourseImagePath);
            DeleteOldFile(course.HandbookFilePath);

            uow.CourseService.Delete(course);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Course deleted successfully."
            });
        }
        //Admin All courses List

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllCoursesForAdmin()
        {
            var courses = await uow.CourseService.GetAllAsync();

            var result = courses.Select(c => new
            {
                c.Id,
                c.Title,
                c.Category,
                c.Description,
                c.HandbookFilePath,
                c.CourseImagePath,
                c.TrainerId,
                TrainerName = c.Trainer != null ? c.Trainer.Name : "No Trainer",
                c.IsPublished,
                c.IsActive,
                c.BatchStartDate,
                c.BatchNumber,
                c.BatchEndDate,
                c.CertificateIncluded,
                c.CreatedAt,
                TotalStudents = c.Enrollments == null ? 0 : c.Enrollments.Count
            });

            return Ok(result);
        }

        [HttpPut("admin/toggle-active/{id}")]
        public async Task<IActionResult> ToggleCourseActiveStatus(int id)
        {
            var course = await uow.CourseService.GetByIdAsync(id);

            if (course == null)
            {
                return NotFound(new APIError(404, "Course not found."));
            }

            course.IsActive = !course.IsActive;

            uow.CourseService.Update(course);
            await uow.SaveAsync();

            return Ok(new
            {
                message = course.IsActive
                    ? "Course activated successfully."
                    : "Course deactivated successfully.",
                isActive = course.IsActive
            });
        }

    }
}