using HTFLMS.Data;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Helper;
using HTFLMS.Models;
using HTFLMS.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HTFLMS.Controllers.Api
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMailService _mail;
        private readonly IPasswordHasher<User> _hasher;

        public EmailController(
            ApplicationDbContext db,
            IMailService mail,
            IPasswordHasher<User> hasher)
        {
            _db = db;
            _mail = mail;
            _hasher = hasher;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "If this email exists, you will receive an OTP shortly."
                });
            }

            var oldOtps = await _db.PasswordResetOtps
                .Where(x => x.UserIdInt == user.Id && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            string otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var resetOtp = new PasswordResetOtp
            {
                UserIdInt = user.Id,
                Email = user.Email,
                OtpHash = HashOtp(otpCode),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
                CreatedAtUtc = DateTime.UtcNow,
                IsUsed = false,
                IsVerified = false,
                Attempts = 0,
                FlowId = Guid.NewGuid().ToString("N")
            };

            _db.PasswordResetOtps.Add(resetOtp);
            await _db.SaveChangesAsync();

            var (subject, body) = EmailTemplates.PasswordResetOtpEmail(otpCode);

            await _mail.SendAsync(user.Email, subject, body);

            return Ok(new
            {
                success = true,
                message = "OTP sent successfully.",
                flowId = resetOtp.FlowId,
                email = user.Email
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _db.PasswordResetOtps
                .FirstOrDefaultAsync(x => x.FlowId == model.FlowId && x.Email == model.Email);

            if (record == null || record.IsUsed || record.ExpiresAtUtc < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP is invalid or expired."
                });
            }

            if (record.Attempts >= 5)
            {
                record.IsUsed = true;
                await _db.SaveChangesAsync();

                return BadRequest(new
                {
                    success = false,
                    message = "Too many attempts. Please request a new OTP."
                });
            }

            record.Attempts++;

            if (record.OtpHash != HashOtp(model.Otp))
            {
                await _db.SaveChangesAsync();

                return BadRequest(new
                {
                    success = false,
                    message = "Incorrect OTP."
                });
            }

            record.IsVerified = true;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "OTP verified successfully.",
                flowId = record.FlowId,
                email = record.Email
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _db.PasswordResetOtps
                .FirstOrDefaultAsync(x => x.FlowId == model.FlowId && x.Email == model.Email);

            if (record == null || record.IsUsed || !record.IsVerified || record.ExpiresAtUtc < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Reset session expired. Please request OTP again."
                });
            }

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == record.UserIdInt);

            if (user == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User not found."
                });
            }

            user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
            record.IsUsed = true;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Password changed successfully. Please login."
            });
        }

        [HttpPost("contact-message")]
        public async Task<IActionResult> ContactMessage([FromBody] ContactMessageDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (subject, body) = EmailTemplates.ContactMessageEmail(
                model.Name,
                model.Email,
                model.Subject,
                model.Message
            );

            await _mail.SendAsync(
                "daudrana400@gmail.com",
                subject,
                body
            );

            return Ok(new
            {
                success = true,
                message = "Your message has been sent successfully."
            });
        }

        private static string HashOtp(string otp)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }
    }
}