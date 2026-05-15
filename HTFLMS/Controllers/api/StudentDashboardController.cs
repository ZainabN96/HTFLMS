using HTFLMS.Data.IServices;
using HTFLMS.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HTFLMS.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public StudentDashboardController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var studentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentIdClaim))
            {
                return Unauthorized(new APIError(401, "Student is not logged in."));
            }

            if (!int.TryParse(studentIdClaim, out int studentId))
            {
                return Unauthorized(new APIError(401, "Invalid student login session."));
            }

            var dashboard = await uow.StudentDashboardService.GetDashboardAsync(studentId);

            return Ok(dashboard);
        }
    }
}