using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/student/enrollment")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentEnrollmentController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMailService mailService;
        private readonly ILogger<StudentEnrollmentController> logger;

        public StudentEnrollmentController(
            IUnitOfWork uow,
            IMailService mailService,
            ILogger<StudentEnrollmentController> logger)
        {
            this.uow = uow;
            this.mailService = mailService;
            this.logger = logger;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll(
            [FromBody] StudentEnrollmentRequestDto dto)
        {
            if (dto.CourseId <= 0)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Invalid course selected."));
            }

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(
                    new APIError(
                        401,
                        "User is not logged in."));
            }

            var student = await uow.UserService
                .GetUserByEmailAsync(email);

            if (student == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Student account was not found."));
            }

            if (!string.Equals(
                    student.MemberType,
                    "Student",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Only students can enroll in courses."));
            }

            if (student.IsActive == false)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Your account is inactive. Please contact admin."));
            }

            var course = await uow.CourseService
                .GetByIdAsync(dto.CourseId);

            if (course == null)
            {
                return NotFound(
                    new APIError(
                        404,
                        "Course not found."));
            }

            if (course.IsActive == false ||
                course.IsPublished == false)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "This course is currently not available for enrollment."));
            }

            var alreadyEnrolled =
                await uow.CourseEnrollmentService
                    .IsAlreadyEnrolledAsync(
                        student.Id,
                        dto.CourseId);

            if (alreadyEnrolled)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "You are already enrolled in this course."));
            }

            var hasAnyActiveEnrollment =
                await uow.CourseEnrollmentService
                    .HasAnyActiveEnrollmentAsync(student.Id);

            if (hasAnyActiveEnrollment)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "You are already enrolled in another active course."));
            }

            var enrollment = new CourseEnrollment
            {
                StudentId = student.Id,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow,
                Status = "Active",
                DeliveryMode =
                    GetDefaultDeliveryMode(student.City)
            };

            uow.CourseEnrollmentService.Add(enrollment);

            var saved = await uow.SaveAsync();

            if (!saved)
            {
                return BadRequest(
                    new APIError(
                        400,
                        "Enrollment could not be completed."));
            }

            // =========================================
            // STUDENT IN-APP + SIGNALR NOTIFICATION
            // =========================================

            try
            {
                await uow.NotificationService.CreateAsync(
                    student.Id,
                    "Enrollment Confirmed",
                    $"You have successfully enrolled in {course.Title}.",
                    "Course",
                    "/Student/Courses");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Student enrollment notification failed. StudentId: {StudentId}, CourseId: {CourseId}",
                    student.Id,
                    course.Id);
            }


            // =========================================
            // TRAINER IN-APP + SIGNALR NOTIFICATION
            // =========================================

            try
            {
                await uow.NotificationService.CreateAsync(
                    course.TrainerId,
                    "New Student Enrollment",
                    $"{student.Name} has enrolled in {course.Title}.",
                    "Course",
                    "/Trainer/Students");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Trainer enrollment notification failed. TrainerId: {TrainerId}, StudentId: {StudentId}, CourseId: {CourseId}",
                    course.TrainerId,
                    student.Id,
                    course.Id);
            }


            // =========================================
            // STUDENT EMAIL
            // =========================================

            try
            {
                await mailService.SendEnrollmentEmailAsync(
                    student.Email,
                    student.Name,
                    course.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Enrollment confirmation email failed. StudentId: {StudentId}, CourseId: {CourseId}",
                    student.Id,
                    course.Id);
            }


            return Ok(new
            {
                message = "Enrollment completed successfully.",
                courseId = dto.CourseId
            });
        }

        private static string GetDefaultDeliveryMode(
            string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return "Onsite";

            return string.Equals(
                    city.Trim(),
                    "Lahore",
                    StringComparison.OrdinalIgnoreCase)
                ? "Onsite"
                : "Online";
        }
    }
}