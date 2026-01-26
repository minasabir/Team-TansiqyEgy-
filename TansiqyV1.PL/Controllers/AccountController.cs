using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Helpers;

namespace TansiqyV1.PL.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "البريد الإلكتروني وكلمة المرور مطلوبان");
            return View();
        }

        // البحث عن المستخدم في قاعدة البيانات
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive && !u.IsDeleted);

        if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            // تحديث آخر تسجيل دخول
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (!string.IsNullOrEmpty(user.FullName))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, user.FullName));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User logged in: {Email}, Role: {Role}", user.Email, user.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Fallback للبيانات القديمة في appsettings.json (للتوافق)
        var adminEmail = _configuration["AdminCredentials:Email"];
        var adminPassword = _configuration["AdminCredentials:Password"];

        if (email == adminEmail && password == adminPassword)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, TansiqyV1.DAL.Enums.UserRole.Admin.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Admin logged in (legacy): {Email}", email);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Admin logged out");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

