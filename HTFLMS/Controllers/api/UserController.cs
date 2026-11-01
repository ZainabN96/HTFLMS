
using AutoMapper;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Helper;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly IPasswordHasher<User> hasher;
        private readonly IMailService mailService;
        private readonly ILogger<UserController> logger;

        public UserController(
            IUnitOfWork uow,
            IMapper mapper,
            IPasswordHasher<User> hasher,
            IMailService mailService,
            ILogger<UserController> logger)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.hasher = hasher;
            this.mailService = mailService;
            this.logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password is required."
                });
            }

            if (await uow.UserService.UserAlreadyExists(dto.CNIC, dto.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User already exists with this CNIC or Email."
                });
            }

            var user = mapper.Map<User>(dto);
            user.TitlePrefix = dto.TitlePrefix;

            user.UserId = await GenerateUniqueUserIdAsync();
            user.MemberType = "Student";
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            user.PasswordHash = hasher.HashPassword(
                user,
                dto.Password
            );

            uow.UserService.Register(user);

            var saved = await uow.SaveAsync();

            if (!saved)
            {
                logger.LogWarning(
                    "Registration database save returned false."
                );

                return BadRequest(new
                {
                    success = false,
                    message = "Registration could not be completed. Please try again."
                });
            }

            // Account has been saved successfully.
            // Email failure must not undo or misreport account creation.

            bool emailSent = false;

            try
            {
                await mailService.SendRegistrationEmailAsync(
                    user.Email,
                    user.Name,
                    user.UserId
                    //dto.Password
                );


                emailSent = true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Registration email failed for UserId {UserId}",
                    user.UserId
                );
            }

            return Ok(new
            {
                success = true,
                emailSent = emailSent,
                message = emailSent
                    ? "Registered successfully! Please login."
                    : "Your account was created successfully, but the registration email could not be sent. Please contact support if you need assistance.",
                userId = user.UserId,
                email = user.Email
            });
        }

        [HttpGet("trainers")]
        public async Task<IActionResult> GetActiveTrainers()
        {
            var users = await uow.UserService.GetAllAsync();

            var trainers = users
                .Where(x => x.MemberType == "Trainer" && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .OrderBy(x => x.Name)
                .ToList();

            return Ok(trainers);
        }

        private async Task<string> GenerateUniqueUserIdAsync()
        {
            var users = await uow.UserService.GetAllAsync();

            int maxNumber = 0;

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.UserId))
                    continue;

                if (user.UserId.StartsWith("HTF"))
                {
                    string numericPart = user.UserId.Substring(3);

                    if (int.TryParse(numericPart, out int number))
                    {
                        if (number > maxNumber)
                            maxNumber = number;
                    }
                }
            }

            int nextNumber = maxNumber + 1;

            return UHelper.GenerateUserId(nextNumber);
        }
    }
}
