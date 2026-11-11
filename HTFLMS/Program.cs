using HTFLMS.Data;
using HTFLMS.Data.IServices;
using HTFLMS.Data.Services;
using HTFLMS.Helper;
using HTFLMS.Hubs;
using HTFLMS.Middlewares;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// QUEST PDF
// =====================================================

QuestPDF.Settings.License =
    LicenseType.Community;


// =====================================================
// MVC / CONTROLLERS
// =====================================================

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


// =====================================================
// AUTHENTICATION
// =====================================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults
            .AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";
    });


// =====================================================
// PASSWORD HASHER
// =====================================================

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();


// =====================================================
// MAIL SERVICE
// =====================================================

builder.Services.Configure<MailSettings>(
    builder.Configuration
        .GetSection("MailSettings"));

builder.Services.AddScoped<
    IMailService,
    MailService>();


// =====================================================
// UNIT OF WORK / SERVICES
// =====================================================

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

builder.Services.AddScoped<
    IModuleService,
    ModuleService>();


// =====================================================
// SIGNALR
// =====================================================

builder.Services.AddSignalR();

builder.Services.AddSingleton<
    IUserIdProvider,
    CustomUserIdProvider>();


// =====================================================
// BACKGROUND / SCHEDULED SERVICES
// =====================================================

builder.Services.AddHostedService<
    AssignmentDueReminderBackgroundService>();

builder.Services.AddHostedService<
    QuizUnlockNotificationService>();


// =====================================================
// AUTOMAPPER
// =====================================================

AutoMapperConfig.RegisterMappings(
    builder.Services);


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// ERROR HANDLING
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


// =====================================================
// MIDDLEWARE
// =====================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// ATTRIBUTE ROUTED API CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// SIGNALR HUB
// =====================================================

app.MapHub<NotificationHub>(
    "/notificationHub");


// =====================================================
// AREA ROUTES
// =====================================================

app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


// =====================================================
// ADMIN ROOT
// =====================================================

app.MapControllerRoute(
    name: "admin-root",
    pattern: "Admin",
    defaults: new
    {
        area = "Admin",
        controller = "Admin",
        action = "Index"
    });


// =====================================================
// TRAINER ROOT
// =====================================================

app.MapControllerRoute(
    name: "trainer-root",
    pattern: "Trainer",
    defaults: new
    {
        area = "Trainer",
        controller = "Trainer",
        action = "Index"
    });


// =====================================================
// DEFAULT ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");


// =====================================================
// RUN
// =====================================================

app.Run();