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

        public AccountController(
            ApplicationDbContext db,
            IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var vm = new LoginViewModel();

            if (TempData.ContainsKey("PrefillEmail"))
            {
                vm.Email = TempData["PrefillEmail"]?.ToString();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var verify = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password
            );

            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim("UserId", user.UserId),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            if (!string.IsNullOrWhiteSpace(user.MemberType))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.MemberType));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            TempData["SuccessMessage"] = "Login successful!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (string.Equals(user.MemberType, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Admin", new { area = "Admin" });

            if (string.Equals(user.MemberType, "Trainer", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Dashboard", new { area = "Trainer" });

            return RedirectToAction("Index", "Student", new { area = "Student" });
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpGet]
        public IActionResult VerifyOtp(string flowId, string email)
        {
            return View(new VerifyOtpViewModel
            {
                FlowId = flowId ?? "",
                Email = email ?? ""
            });
        }

        [HttpGet]
        public IActionResult ResetPassword(string flowId, string email)
        {
            return View(new ResetPasswordViewModel
            {
                FlowId = flowId ?? "",
                Email = email ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            TempData["SuccessMessage"] = "Logged out successfully!";

            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}