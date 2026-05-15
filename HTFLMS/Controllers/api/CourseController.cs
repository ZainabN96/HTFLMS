using AutoMapper;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Helpers;
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
            course.DurationText = dto.DurationText;
            course.CertificateIncluded = dto.CertificateIncluded;
            course.CreatedAt = DateTime.UtcNow;

            FileUploadHelper.ApplyCourseStatus(course, dto.Status);

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.ImageFile,
                null,
                "uploads/courses/images",
                env,
                path => course.CourseImagePath = path
            );

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.HandbookFile,
                null,
                "uploads/courses/handbooks",
                env,
                path => course.HandbookFilePath = path
            );

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
                c.DurationText,
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
                return NotFound(new APIError(404, "Course not found."));

            return Ok(course);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] CourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = await uow.CourseService.GetByIdAsync(id);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            course.Title = dto.Title;
            course.Category = dto.Category;
            course.Description = dto.Description;
            course.TrainerId = dto.TrainerId;
            course.BatchStartDate = dto.BatchStartDate;
            course.BatchEndDate = dto.BatchEndDate;
            course.BatchNumber = dto.BatchNumber;
            course.DurationText = dto.DurationText;
            course.CertificateIncluded = dto.CertificateIncluded;

            FileUploadHelper.ApplyCourseStatus(course, dto.Status);

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.ImageFile,
                course.CourseImagePath,
                "uploads/courses/images",
                env,
                path => course.CourseImagePath = path
            );

            await FileUploadHelper.ReplaceFileIfUploadedAsync(
                dto.HandbookFile,
                course.HandbookFilePath,
                "uploads/courses/handbooks",
                env,
                path => course.HandbookFilePath = path
            );

            uow.CourseService.Update(course);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Course updated successfully.",
                courseId = course.Id
            });
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

            FileUploadHelper.DeleteOldFile(course.CourseImagePath, env);
            FileUploadHelper.DeleteOldFile(course.HandbookFilePath, env);

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