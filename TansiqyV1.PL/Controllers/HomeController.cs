using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Enums;
using TansiqyV1.PL.Models;

namespace TansiqyV1.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
        public async Task<IActionResult> Index()
        {
            // جلب عدد الجامعات الحقيقي لكل نوع - استعلام واحد محسّن
            var counts = await _context.Universities
                .Where(u => !u.IsDeleted)
                .GroupBy(u => u.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var universityCounts = new Dictionary<UniversityType, int>
            {
                { UniversityType.Governmental, counts.FirstOrDefault(c => c.Type == UniversityType.Governmental)?.Count ?? 0 },
                { UniversityType.Private, counts.FirstOrDefault(c => c.Type == UniversityType.Private)?.Count ?? 0 },
                { UniversityType.National, counts.FirstOrDefault(c => c.Type == UniversityType.National)?.Count ?? 0 },
                { UniversityType.Technological, counts.FirstOrDefault(c => c.Type == UniversityType.Technological)?.Count ?? 0 },
                { UniversityType.Foreign, counts.FirstOrDefault(c => c.Type == UniversityType.Foreign)?.Count ?? 0 },
                { UniversityType.HigherInstitute, counts.FirstOrDefault(c => c.Type == UniversityType.HigherInstitute)?.Count ?? 0 }
            };

            ViewBag.UniversityCounts = universityCounts;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
