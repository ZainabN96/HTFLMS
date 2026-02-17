using HTFLMS.Data;
using HTFLMS.Models;
using HTFLMS.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HTFLMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext db, IPasswordHasher<User> hasher, IWebHostEnvironment env, HTFLMS.Services.IMailService mail)
        {
            _db = db;
            _hasher = hasher;
            _env = env;
            _mail = mail;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var vm = new LoginViewModel();

            // changed: prefill email after registration
            if (TempData.ContainsKey("PrefillEmail"))
                vm.Email = TempData["PrefillEmail"]?.ToString();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            // changed: login by email
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Cookie sign-in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? ""),
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

            // role-based redirect (Areas)
            if (string.Equals(user.MemberType, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (string.Equals(user.MemberType, "Trainer", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Dashboard", new { area = "Trainer" });

            return RedirectToAction("CourseHome", "Courses");

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
                Name = model.Name,
                MemberType = "Student",
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

            // changed: prefill email on login page after registration
            TempData["PrefillEmail"] = model.Email;

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

        private readonly HTFLMS.Services.IMailService _mail;

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        // Always show success message (don’t reveal if email exists)
        TempData["SuccessMessage"] = "If this email exists, you will receive an OTP shortly.";

        if (user == null) return RedirectToAction(nameof(VerifyOtp), new { flowId = "", email = model.Email });

        // Optional: invalidate old OTPs for this user
        var old = await _db.PasswordResetOtps
            .Where(x => x.UserIdInt == user.Id && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();
        foreach (var x in old) x.IsUsed = true;

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString(); // 6-digit
        var flow = new PasswordResetOtp
        {
            UserIdInt = user.Id,
            Email = user.Email,
            OtpHash = HashOtp(otp),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            IsVerified = false,
            Attempts = 0,
            FlowId = Guid.NewGuid().ToString("N")
        };

        _db.PasswordResetOtps.Add(flow);
        await _db.SaveChangesAsync();

        var body = $@"
        <div style='font-family:Arial'>
            <h3>Password Reset OTP</h3>
            <p>Your OTP is:</p>
            <h2 style='letter-spacing:2px'>{otp}</h2>
            <p>This code expires in 10 minutes.</p>
            <p>If you did not request this, ignore this email.</p>
        </div>";

        await _mail.SendAsync(user.Email, "HTF LMS - Password Reset OTP", body);

        return RedirectToAction(nameof(VerifyOtp), new { flowId = flow.FlowId, email = user.Email });
    }

    [HttpGet]
    public IActionResult VerifyOtp(string flowId, string email)
    {
        return View(new VerifyOtpViewModel { FlowId = flowId ?? "", Email = email ?? "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var record = await _db.PasswordResetOtps
            .FirstOrDefaultAsync(x => x.FlowId == model.FlowId && x.Email == model.Email);

        if (record == null || record.IsUsed || record.ExpiresAtUtc < DateTime.UtcNow)
        {
            ModelState.AddModelError("", "OTP is invalid or expired. Please request a new one.");
            return View(model);
        }

        if (record.Attempts >= 5)
        {
            record.IsUsed = true;
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Too many attempts. Please request a new OTP.");
            return View(model);
        }

        record.Attempts += 1;

        if (record.OtpHash != HashOtp(model.Otp))
        {
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Incorrect OTP.");
            return View(model);
        }

        record.IsVerified = true;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ResetPassword), new { flowId = record.FlowId, email = record.Email });
    }

    [HttpGet]
    public IActionResult ResetPassword(string flowId, string email)
    {
        return View(new ResetPasswordViewModel { FlowId = flowId ?? "", Email = email ?? "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var record = await _db.PasswordResetOtps
            .FirstOrDefaultAsync(x => x.FlowId == model.FlowId && x.Email == model.Email);

        if (record == null || record.IsUsed || !record.IsVerified || record.ExpiresAtUtc < DateTime.UtcNow)
        {
            ModelState.AddModelError("", "Reset session expired. Please request OTP again.");
            return View(model);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == record.UserIdInt);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);

        record.IsUsed = true; // invalidate OTP
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Password changed successfully. Please login.";
        return RedirectToAction(nameof(Login));
    }

    private static string HashOtp(string otp)
    {
        // simple hash; you can also use HMAC with a secret key if you want
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(bytes);
    }


}
}
