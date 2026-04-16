using HTFLMS.Data;
using HTFLMS.Models;
using HTFLMS.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageTrainersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public ManageTrainersController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // Trainers List Page
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _db.Users
                .Where(u => u.MemberType == "Trainer")
                .ToListAsync();

            return View(trainers);
        }

        // GET: Open form
        [HttpGet]
        public IActionResult AddTrainer()
        {
            return View(new AddTrainerViewModel());
        }

        // POST: Save trainer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrainer(AddTrainerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Email check
            var emailExists = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            // CNIC check
            var cnicExists = await _db.Users.AnyAsync(u => u.CNIC == model.CNIC);
            if (cnicExists)
            {
                ModelState.AddModelError("CNIC", "CNIC already exists.");
                return View(model);
            }

            // Generate UserId
            string userId = GenerateUserId();
            while (await _db.Users.AnyAsync(u => u.UserId == userId))
            {
                userId = GenerateUserId();
            }

            // Create trainer
            var trainer = new User
            {
                UserId = userId,
                Name = model.Name,
                Email = model.Email,
                Designation = model.Designation,
                Gender = model.Gender,
                Qualification = model.Qualification,
                CNIC = model.CNIC,
                Address = model.Address,
                MobileNumber = model.MobileNumber,
                IsActive = model.IsActive ?? true,
                MemberType = "Trainer",
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            trainer.PasswordHash = _hasher.HashPassword(trainer, model.Password);

            // Save to DB
            _db.Users.Add(trainer);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trainer added successfully.";

            return RedirectToAction("Trainers");
        }

        private string GenerateUserId()
        {
            return $"TR-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
        }
    }
}