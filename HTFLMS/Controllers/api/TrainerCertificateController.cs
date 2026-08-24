using HTFLMS.Data.IServices;
using HTFLMS.Dtos.CertificateReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HTFLMS.Controllers.api
{
    [Route("api/trainer/certificates")]
    [ApiController]
    [Authorize(Roles = "Trainer")]
    public class TrainerCertificateController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public TrainerCertificateController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("review")]
        public async Task<IActionResult> GetReview(
            [FromQuery] int? courseId,
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerCertificateService.GetReviewAsync(
                trainerId,
                courseId,
                search,
                status
            );

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Certificate review data was not found or you are not allowed to access this course."
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
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerCertificateService.ApproveRequestAsync(
                trainerId,
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
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerCertificateService.RejectRequestAsync(
                trainerId,
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
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
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

            var result = await uow.TrainerCertificateService.UpdateDeliveryModeAsync(
                trainerId,
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

        [HttpPost("course/{courseId:int}/generate")]
        public async Task<IActionResult> GenerateCertificates(int courseId)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerCertificateService.GenerateCertificatesAsync(
                trainerId,
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
        private int GetLoggedInTrainerId()
        {
            var trainerIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("UserId");

            if (string.IsNullOrWhiteSpace(trainerIdClaim))
            {
                return 0;
            }

            return int.TryParse(trainerIdClaim, out var trainerId)
                ? trainerId
                : 0;
        }
    }
}