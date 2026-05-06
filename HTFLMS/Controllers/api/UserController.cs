using AutoMapper;
using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Helper;
using HTFLMS.Helpers;
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

        public UserController(IUnitOfWork uow, IMapper mapper, IPasswordHasher<User> hasher)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.hasher = hasher;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto dto)
        {
            // 1. ModelState validation (handles [Required] etc.)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Duplicate check
            if (await uow.UserService.UserAlreadyExists(dto.CNIC, dto.Email))
            {
                return BadRequest(new APIError(
                    BadRequest().StatusCode,
                    "User already exists with this CNIC or Email."
                ));
            }

            // 3. Map DTO -> User
            var user = mapper.Map<User>(dto);

            // 4. Generate unique UserId
            user.UserId = await GenerateUniqueUserIdAsync();

            // 5. Hash password
            user.PasswordHash = hasher.HashPassword(user, dto.Password);
            // 6. Save
            uow.UserService.Register(user);
            await uow.SaveAsync();

            return Ok(new { message = "User registered successfully.", userId = user.UserId });
        }

        //trainer dropdown list by sb 
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

        // ── private helper ────────────────────────────────────────────
        private async Task<string> GenerateUniqueUserIdAsync()
        {
            string id;
            do { id = UHelper.GenerateUserId(); }
            while (await uow.UserService.UserIdExists(id));
            return id;
        }
    }
}