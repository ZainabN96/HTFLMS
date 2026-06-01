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

        public UserController(
            IUnitOfWork uow,
            IMapper mapper,
            IPasswordHasher<User> hasher,
            IMailService mailService)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.hasher = hasher;
            this.mailService = mailService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await uow.UserService.UserAlreadyExists(dto.CNIC, dto.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User already exists with this CNIC or Email."
                });
            }

            var user = mapper.Map<User>(dto);

            user.UserId = await GenerateUniqueUserIdAsync();
            user.MemberType = "Student";
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            uow.UserService.Register(user);
            var saved = await uow.SaveAsync();

            if (!saved)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Registration failed. Please try again."
                });
            }

            await mailService.SendRegistrationEmailAsync(
                user.Email,
                user.Name,
                user.UserId,
                dto.Password
            );

            return Ok(new
            {
                success = true,
                message = "Registered successfully! Please login.",
                userId = user.UserId,
                email = user.Email
            });
        }

        [HttpGet("trainers")]
        public async Task<IActionResult> GetActiveTrainers()
        {
            var users = await uow.UserService.GetAllAsync();

            var trainers = users
                .Where(x => x.MemberType == "Trainer" && x.IsActive == true)
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