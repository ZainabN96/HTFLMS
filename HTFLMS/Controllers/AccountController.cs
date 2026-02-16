using HTFLMS.Data;
using HTFLMS.Models;
using HTFLMS.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HTFLMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext db, IPasswordHasher<User> hasher, IWebHostEnvironment env)
        {
            _db = db;
            _hasher = hasher;
            _env = env;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var vm = new LoginViewModel();

            if (TempData.ContainsKey("PrefillUserId"))
                vm.UserId = TempData["PrefillUserId"]?.ToString();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid User Id or password.");
                return View(model);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid User Id or password.");
                return View(model);
            }

            // Cookie sign-in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserId),
                new Claim("UserId", user.UserId),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            if (!string.IsNullOrWhiteSpace(user.MemberType))
                claims.Add(new Claim(ClaimTypes.Role, user.MemberType));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = "Login successful!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("CoursesIndex", "Courses");
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            if (model.MemberType != "Student" && model.MemberType != "Trainer")
            {
                ModelState.AddModelError("MemberType", "Invalid member type selected.");
                return View(model);
            }

            // Email must be unique
            var emailExists = await _db.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            // Generate only UserId automatically
            string generatedUserId = GenerateUserId();
            while (await _db.Users.AnyAsync(u => u.UserId == generatedUserId))
                generatedUserId = GenerateUserId();

            var user = new User
            {
                UserId = generatedUserId,
                Email = model.Email,

                Gender = model.Gender,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                MemberType = model.MemberType,
                Qualification = model.Qualification,
                CNIC = model.CNIC,

                Address = model.Address,
                Country = model.Country,
                City = model.City,

                MobileNumber = model.MobileNumber,
                LinkedIn = model.LinkedIn,
                EmploymentStatus = model.EmploymentStatus
            };

            user.PasswordHash = _hasher.HashPassword(user, model.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registered successfully! Please login.";
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        private static string GenerateUserId()
        {
            return $"HCC-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}
