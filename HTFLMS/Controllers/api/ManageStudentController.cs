using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/manage-student")]
    [ApiController]
    public class ManageStudentController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly PasswordHasher<User> hasher;

        public ManageStudentController(IUnitOfWork uow)
        {
            this.uow = uow;
            hasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var students = await uow.ManageStudentService.GetAllForUserAsync(email);

            return Ok(students);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await uow.ManageStudentService.GetForEditAsync(id);

            if (student == null)
                return NotFound(new APIError(404, "Student not found."));

            return Ok(student);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] ManageStudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CourseIds == null || dto.CourseIds.Count == 0)
                return BadRequest(new APIError(400, "Please select at least one course."));

            var courseAccessError = await ValidateCourseAccessAsync(dto.CourseIds);

            if (courseAccessError != null)
                return courseAccessError;

            var existingUser = await uow.ManageStudentService.GetAnyUserByEmailAsync(dto.Email);

            if (existingUser != null &&
                !string.Equals(existingUser.MemberType, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new APIError(400, "This email already belongs to another user type."));
            }

            var student = await uow.ManageStudentService.GetStudentByEmailAsync(dto.Email);

            if (student == null)
            {
                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new APIError(400, "Password is required for new student."));

                var nextNumber = await uow.ManageStudentService.GetNextStudentNumberAsync();

                student = new User
                {
                    UserId = GenerateUserId(nextNumber),
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim(),
                    MemberType = "Student",
                    IsActive = dto.Status == "Active",
                    CreatedAt = dto.JoinDate ?? DateTime.UtcNow,

                    CNIC = "N/A",
                    MobileNumber = "N/A",
                    Gender = "",
                    Qualification = "",
                    Address = "",
                    Country = "",
                    City = "",
                    LinkedIn = "",
                    EmploymentStatus = ""
                };

                student.PasswordHash = hasher.HashPassword(student, dto.Password);

                uow.ManageStudentService.Add(student);
                await uow.SaveAsync();
            }
            else
            {
                student.Name = dto.Name.Trim();
                student.IsActive = dto.Status == "Active";

                if (dto.JoinDate.HasValue)
                    student.CreatedAt = dto.JoinDate.Value;

                if (!string.IsNullOrWhiteSpace(dto.Password))
                    student.PasswordHash = hasher.HashPassword(student, dto.Password);

                uow.ManageStudentService.Update(student);
                await uow.SaveAsync();
            }

            await uow.ManageStudentService.EnrollStudentInCoursesAsync(student.Id, dto.CourseIds);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Student saved and enrolled successfully.",
                studentId = student.Id,
                userId = student.UserId
            });
        }

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] ManageStudentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CourseIds == null || dto.CourseIds.Count == 0)
                return BadRequest(new APIError(400, "Please select at least one course."));

            var courseAccessError = await ValidateCourseAccessAsync(dto.CourseIds);

            if (courseAccessError != null)
                return courseAccessError;

            var student = await uow.ManageStudentService.GetStudentByIdAsync(id);

            if (student == null)
                return NotFound(new APIError(404, "Student not found."));

            var existingUser = await uow.ManageStudentService.GetAnyUserByEmailAsync(dto.Email);

            if (existingUser != null && existingUser.Id != id)
                return BadRequest(new APIError(400, "Email already belongs to another user."));

            student.Name = dto.Name.Trim();
            student.Email = dto.Email.Trim();
            student.IsActive = dto.Status == "Active";

            if (dto.JoinDate.HasValue)
                student.CreatedAt = dto.JoinDate.Value;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                student.PasswordHash = hasher.HashPassword(student, dto.Password);

            uow.ManageStudentService.Update(student);

            await uow.ManageStudentService.EnrollStudentInCoursesAsync(student.Id, dto.CourseIds);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Student updated and enrolled successfully.",
                studentId = student.Id,
                userId = student.UserId
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await uow.ManageStudentService.GetStudentByIdAsync(id);

            if (student == null)
                return NotFound(new APIError(404, "Student not found."));

            student.IsActive = false;

            uow.ManageStudentService.Update(student);
            await uow.SaveAsync();

            return Ok(new
            {
                message = "Student deactivated successfully."
            });
        }

        private async Task<IActionResult?> ValidateCourseAccessAsync(List<int> courseIds)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var allowedCourseIds = await uow.ManageStudentService
                .GetAllowedCourseIdsForUserAsync(email);

            if (allowedCourseIds.Count == 0)
                return Unauthorized(new APIError(401, "You are not allowed to assign courses."));

            var invalidCourseIds = courseIds
                .Where(id => !allowedCourseIds.Contains(id))
                .ToList();

            if (invalidCourseIds.Any())
            {
                return Unauthorized(new APIError(
                    401,
                    "You can only assign students to courses that are allowed for your role."
                ));
            }

            return null;
        }

        private static string GenerateUserId(int nextNumber)
        {
            return $"HTF{nextNumber:D3}";
        }
    }
}