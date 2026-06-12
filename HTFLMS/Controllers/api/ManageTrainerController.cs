using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageTrainerController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher<User> hasher;
        private readonly IWebHostEnvironment env;

        public ManageTrainerController(
            IUnitOfWork unitOfWork,
            IPasswordHasher<User> hasher,
            IWebHostEnvironment env)
        {
            this.unitOfWork = unitOfWork;
            this.hasher = hasher;
            this.env = env;
        }

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAll()
        {
            var trainers = await unitOfWork.ManageTrainerService.GetAllAsync();

            var result = new List<ManageTrainerDto>();

            foreach (var trainer in trainers)
            {
                result.Add(await ToDto(trainer));
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trainer = await unitOfWork.ManageTrainerService.GetByIdAsync(id);

            if (trainer == null)
                return NotFound(new { message = "Trainer not found." });

            return Ok(await ToDto(trainer));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ManageTrainerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                ModelState.AddModelError(nameof(dto.Password), "Password is required.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                ModelState.AddModelError(nameof(dto.ConfirmPassword), "Confirm Password is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await unitOfWork.ManageTrainerService.EmailExistsAsync(dto.Email))
                return BadRequest(new { message = "Email already exists." });

            if (await unitOfWork.ManageTrainerService.CnicExistsAsync(dto.CNIC))
                return BadRequest(new { message = "CNIC already exists." });

            var trainer = new User
            {
                UserId = await unitOfWork.ManageTrainerService.GenerateUniqueUserIdAsync(),
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                Designation = dto.Designation.Trim(),
                CNIC = dto.CNIC.Trim(),
                MobileNumber = dto.MobileNumber.Trim(),
                IsActive = dto.IsActive ?? true,
                Gender = dto.Gender,
                Qualification = dto.Qualification,
                Address = dto.Address,
                MemberType = "Trainer",
                CreatedAt = DateTime.Now
            };

            if (dto.Picture != null)
                trainer.ProfilePicturePath = await SaveTrainerPicture(dto.Picture);

            trainer.PasswordHash = hasher.HashPassword(trainer, dto.Password!);

            unitOfWork.ManageTrainerService.Add(trainer);

            var saved = await unitOfWork.SaveAsync();

            if (!saved)
                return BadRequest(new { message = "Trainer could not be added." });

            return Ok(new { message = "Trainer added successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ManageTrainerDto dto)
        {
            ModelState.Remove(nameof(dto.Password));
            ModelState.Remove(nameof(dto.ConfirmPassword));

            if (!string.IsNullOrWhiteSpace(dto.Password) &&
                dto.Password != dto.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(dto.ConfirmPassword), "Password and Confirm Password do not match.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var trainer = await unitOfWork.ManageTrainerService.GetByIdAsync(id);

            if (trainer == null)
                return NotFound(new { message = "Trainer not found." });

            if (await unitOfWork.ManageTrainerService.EmailExistsAsync(dto.Email, id))
                return BadRequest(new { message = "Email already exists." });

            if (await unitOfWork.ManageTrainerService.CnicExistsAsync(dto.CNIC, id))
                return BadRequest(new { message = "CNIC already exists." });

            trainer.Name = dto.Name.Trim();
            trainer.Email = dto.Email.Trim();
            trainer.Designation = dto.Designation.Trim();
            trainer.CNIC = dto.CNIC.Trim();
            trainer.MobileNumber = dto.MobileNumber.Trim();
            trainer.IsActive = dto.IsActive ?? true;
            trainer.Gender = dto.Gender;
            trainer.Qualification = dto.Qualification;
            trainer.Address = dto.Address;

            if (dto.Picture != null)
            {
                DeleteOldPicture(trainer.ProfilePicturePath);
                trainer.ProfilePicturePath = await SaveTrainerPicture(dto.Picture);
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                trainer.PasswordHash = hasher.HashPassword(trainer, dto.Password);

            unitOfWork.ManageTrainerService.Update(trainer);

            var saved = await unitOfWork.SaveAsync();

            if (!saved)
                return BadRequest(new { message = "Trainer could not be updated." });

            return Ok(new { message = "Trainer updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trainer = await unitOfWork.ManageTrainerService.GetByIdAsync(id);

            if (trainer == null)
                return NotFound(new { message = "Trainer not found." });

            DeleteOldPicture(trainer.ProfilePicturePath);

            unitOfWork.ManageTrainerService.Delete(trainer);

            var saved = await unitOfWork.SaveAsync();

            if (!saved)
                return BadRequest(new { message = "Trainer could not be deleted." });

            return Ok(new { message = "Trainer deleted successfully." });
        }

        private async Task<string> SaveTrainerPicture(IFormFile picture)
        {
            var folder = Path.Combine(env.WebRootPath, "uploads", "trainers");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(picture.FileName);
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await picture.CopyToAsync(stream);

            return "/uploads/trainers/" + fileName;
        }

        private void DeleteOldPicture(string? picturePath)
        {
            if (string.IsNullOrWhiteSpace(picturePath))
                return;

            var fullPath = Path.Combine(
                env.WebRootPath,
                picturePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        private async Task<ManageTrainerDto> ToDto(User trainer)
        {
            return new ManageTrainerDto
            {
                Id = trainer.Id,
                Name = trainer.Name,
                Email = trainer.Email,
                Designation = trainer.Designation,
                CNIC = trainer.CNIC,
                MobileNumber = trainer.MobileNumber,
                IsActive = trainer.IsActive,
                Gender = trainer.Gender,
                Qualification = trainer.Qualification,
                Address = trainer.Address,
                ProfilePicturePath = trainer.ProfilePicturePath,
                CreatedAt = trainer.CreatedAt,
                AssignedCourseCount = await unitOfWork.ManageTrainerService
                    .GetAssignedCourseCountAsync(trainer.Id)
            };
        }
    }
}