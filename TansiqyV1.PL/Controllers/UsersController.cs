using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;
using TansiqyV1.DAL.Helpers;

namespace TansiqyV1.PL.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return View(users);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // التحقق من عدم وجود مستخدم بنفس البريد الإلكتروني
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                FullName = model.FullName,
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created: {Email}, Role: {Role}", user.Email, user.Role);
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound();
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive
        };

        return View(model);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                return NotFound();
            }

            // التحقق من البريد الإلكتروني إذا تم تغييره
            if (user.Email != model.Email)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != id && !u.IsDeleted);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");
                    return View(model);
                }
            }

            user.Email = model.Email;
            user.FullName = model.FullName;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            // تحديث كلمة المرور فقط إذا تم إدخال واحدة جديدة
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = PasswordHelper.HashPassword(model.Password);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated: {Email}", user.Email);
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: Users/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
        {
            return NotFound();
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User deleted: {Email}", user.Email);
        return RedirectToAction(nameof(Index));
    }
}

// ViewModels
public class CreateUserViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "الاسم الكامل")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "الدور مطلوب")]
    [Display(Name = "الدور")]
    public UserRole Role { get; set; } = UserRole.Student;

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;
}

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
    [Display(Name = "كلمة المرور (اتركه فارغاً إذا لم ترد تغييره)")]
    public string? Password { get; set; }

    [Display(Name = "الاسم الكامل")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "الدور مطلوب")]
    [Display(Name = "الدور")]
    public UserRole Role { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; }
}

