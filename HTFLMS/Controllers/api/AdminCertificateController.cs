using HTFLMS.Data.IServices;
using HTFLMS.Dtos.CertificateReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HTFLMS.Controllers.api
{
    [Route("api/admin/certificates")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCertificateController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public AdminCertificateController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("review")]
        public async Task<IActionResult> GetReview(
            [FromQuery] int? courseId,
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var adminId = GetLoggedInAdminId();

            if (adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Admin login not found."
                });
            }

            var result = await uow.AdminCertificateService.GetReviewAsync(
                courseId,
                search,
                status
            );

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Certificate review data was not found."
                });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost("{certificateRequestId:int}/approve")]
        public async Task<IActionResult> ApproveCertificateRequest(int certificateRequestId)
        {
            var adminId = GetLoggedInAdminId();

            if (adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Admin login not found."
                });
            }

            var result = await uow.AdminCertificateService.ApproveRequestAsync(
                adminId,
                certificateRequestId
            );

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        [HttpPost("{certificateRequestId:int}/reject")]
        public async Task<IActionResult> RejectCertificateRequest(int certificateRequestId)
        {
            var adminId = GetLoggedInAdminId();

            if (adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Admin login not found."
                });
            }

            var result = await uow.AdminCertificateService.RejectRequestAsync(
                adminId,
                certificateRequestId
            );

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        [HttpPut("enrollment/{enrollmentId:int}/delivery-mode")]
        public async Task<IActionResult> UpdateDeliveryMode(
            int enrollmentId,
            [FromBody] CertificateReviewDeliveryModeUpdateDto dto)
        {
            var adminId = GetLoggedInAdminId();

            if (adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Admin login not found."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Delivery mode must be Online or Onsite."
                });
            }

            var result = await uow.AdminCertificateService.UpdateDeliveryModeAsync(
                adminId,
                enrollmentId,
                dto.DeliveryMode);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        private int GetLoggedInAdminId()
        {
            var adminIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("UserId");

            if (string.IsNullOrWhiteSpace(adminIdClaim))
            {
                return 0;
            }

            return int.TryParse(adminIdClaim, out var adminId)
                ? adminId
                : 0;
        }
        [HttpPost("course/{courseId:int}/generate")]
        public async Task<IActionResult> GenerateCertificates(int courseId)
        {
            var adminId = GetLoggedInAdminId();

            if (adminId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Admin login not found."
                });
            }

            var result = await uow.AdminCertificateService.GenerateCertificatesAsync(
                adminId,
                courseId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    generatedCount = result.GeneratedCount,
                    skippedCount = result.SkippedCount,
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                generatedCount = result.GeneratedCount,
                skippedCount = result.SkippedCount,
                generatedCertificates = result.GeneratedCertificates
            });
        }
    }
}