using System.Security.Cryptography;
using System.Text;

namespace TansiqyV1.DAL.Helpers;

public static class PasswordHelper
{
    // متغير لتحديد ما إذا كان يجب Hash كلمة المرور أم لا
    // ⚠️ تحذير: تعطيل Hash غير آمن ويجب استخدامه للاختبار فقط!
    public static bool UseHash { get; set; } = true; // true = مع Hash (آمن)

    /// <summary>
    /// Hash password using SHA256 (أو إرجاعها كما هي إذا كان UseHash = false)
    /// </summary>
    public static string HashPassword(string password)
    {
        if (!UseHash)
        {
            // للاختبار: إرجاع كلمة المرور كما هي
            return password;
        }

        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Verify password against hash (أو مقارنة مباشرة إذا كان UseHash = false)
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        if (!UseHash)
        {
            // للاختبار: مقارنة مباشرة
            return password == hash;
        }

        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }
}

