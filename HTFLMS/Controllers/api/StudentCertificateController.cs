using System.Security.Claims;
using HTFLMS.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/student/certificates")]
    public class StudentCertificateController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public StudentCertificateController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetCertificates()
        {
            var studentId = GetCurrentUserId();

            if (studentId <= 0)
                return Unauthorized(new { success = false, message = "User is not logged in." });

            var certificates = await unitOfWork.StudentCertificateService.GetCertificatesAsync(studentId);

            return Ok(new
            {
                success = true,
                data = certificates
            });
        }

        [HttpPost("apply/{courseId:int}")]
        public async Task<IActionResult> Apply(int courseId)
        {
            var studentId = GetCurrentUserId();

            if (studentId <= 0)
                return Unauthorized(new { success = false, message = "User is not logged in." });

            var result = await unitOfWork.StudentCertificateService.ApplyAsync(studentId, courseId);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet("{certificateRequestId:int}")]
        public async Task<IActionResult> GetCertificateDetail(int certificateRequestId)
        {
            var studentId = GetCurrentUserId();

            if (studentId <= 0)
                return Unauthorized(new { success = false, message = "User is not logged in." });

            var certificate = await unitOfWork.StudentCertificateService
                .GetCertificateDetailAsync(studentId, certificateRequestId);

            if (certificate == null)
                return NotFound(new { success = false, message = "Certificate was not found." });

            return Ok(new
            {
                success = true,
                data = certificate
            });
        }

        private int GetCurrentUserId()
        {
            var claimValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("UserId");

            return int.TryParse(claimValue, out var userId) ? userId : 0;
        }
    }
}