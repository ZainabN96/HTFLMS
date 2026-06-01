using HTFLMS.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HTFLMS.Controllers.API
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentGradesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public StudentGradesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentGrades()
        {
            var studentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentIdClaim) || !int.TryParse(studentIdClaim, out var studentId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Student session is invalid. Please login again."
                });
            }

            var result = await unitOfWork.StudentGradesService.GetGradesPageAsync(studentId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}