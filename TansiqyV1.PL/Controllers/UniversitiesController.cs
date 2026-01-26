using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.PL.Controllers;

public class UniversitiesController : Controller
{
    private readonly IUniversityService _universityService;

    public UniversitiesController(IUniversityService universityService)
    {
        _universityService = universityService;
    }

    // GET: Universities
    [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "type" })]
    public async Task<IActionResult> Index(int? type)
    {
        if (!type.HasValue)
        {
            return RedirectToAction("SelectType");
        }

        var universities = await _universityService.GetUniversitiesByTypeAsync((UniversityType)type.Value);
        ViewBag.UniversityType = type.Value;
        return View(universities);
    }

    // GET: Universities/SelectType
    public IActionResult SelectType()
    {
        return View();
    }

    // GET: Universities/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var university = await _universityService.GetUniversityByIdAsync(id);
        if (university == null)
        {
            return NotFound();
        }
        return View(university);
    }

    // GET: Universities/Colleges/5
    public async Task<IActionResult> Colleges(int id)
    {
        var colleges = await _universityService.GetCollegesByUniversityIdAsync(id);
        ViewBag.UniversityId = id;
        return View(colleges);
    }

    // GET: Universities/CollegeDetails/5
    public async Task<IActionResult> CollegeDetails(int id)
    {
        var college = await _universityService.GetCollegeByIdAsync(id);
        if (college == null)
        {
            return NotFound();
        }
        return View(college);
    }

    // GET: Universities/Departments/5
    public async Task<IActionResult> Departments(int id)
    {
        var college = await _universityService.GetCollegeByIdAsync(id);
        if (college == null)
        {
            return NotFound();
        }
        ViewBag.CollegeId = id;
        ViewBag.CollegeName = college.NameAr;
        ViewBag.UniversityId = college.University?.Id;
        ViewBag.UniversityName = college.University?.NameAr;
        return View(college.Departments);
    }

    // GET: Universities/Search
    public async Task<IActionResult> Search(
        string? searchTerm,
        int? type,
        int? governorate,
        int? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName)
    {
        var universities = await _universityService.SearchUniversitiesAsync(
            searchTerm,
            type.HasValue ? (UniversityType?)type.Value : null,
            governorate.HasValue ? (Governorate?)governorate.Value : null,
            studyType.HasValue ? (StudyType?)studyType.Value : null,
            minFees,
            maxFees,
            minCoordination,
            maxCoordination,
            collegeName);

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Type = type;
        ViewBag.Governorate = governorate;
        ViewBag.StudyType = studyType;
        ViewBag.MinFees = minFees;
        ViewBag.MaxFees = maxFees;
        ViewBag.MinCoordination = minCoordination;
        ViewBag.MaxCoordination = maxCoordination;
        ViewBag.CollegeName = collegeName;

        return View(universities);
    }

    // Admin CRUD Actions
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BLL.ModelVM.CreateUniversityDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var university = await _universityService.CreateUniversityAsync(dto);
                return RedirectToAction("Details", new { id = university.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
            }
        }
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var university = await _universityService.GetUniversityByIdAsync(id);
        if (university == null)
        {
            return NotFound();
        }

        var dto = new BLL.ModelVM.UpdateUniversityDto
        {
            Id = university.Id,
            NameAr = university.NameAr,
            NameEn = university.NameEn,
            Type = university.Type,
            OfficialWebsite = university.OfficialWebsite,
            Location = university.Location,
            Governorate = university.Governorate,
            LastYearCoordination = university.LastYearCoordination,
            Fees = university.Fees,
            InformationSources = university.InformationSources,
            Description = university.Description
        };

        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BLL.ModelVM.UpdateUniversityDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var university = await _universityService.UpdateUniversityAsync(dto);
                return RedirectToAction("Details", new { id = university.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
            }
        }
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _universityService.DeleteUniversityAsync(id);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest($"حدث خطأ: {ex.Message}");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult CreateCollege(int universityId)
    {
        ViewBag.UniversityId = universityId;
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCollege(BLL.ModelVM.CreateCollegeDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var college = await _universityService.CreateCollegeAsync(dto);
                return RedirectToAction("CollegeDetails", new { id = college.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
            }
        }
        ViewBag.UniversityId = dto.UniversityId;
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditCollege(int id)
    {
        var college = await _universityService.GetCollegeByIdAsync(id);
        if (college == null)
        {
            return NotFound();
        }

        var dto = new BLL.ModelVM.UpdateCollegeDto
        {
            Id = college.Id,
            NameAr = college.NameAr,
            NameEn = college.NameEn,
            UniversityId = college.UniversityId,
            OfficialWebsite = college.OfficialWebsite,
            Location = college.Location,
            Description = college.Description,
            Fees = college.Fees,
            LastYearCoordination = college.LastYearCoordination,
            FeesCategoryA = college.FeesCategoryA,
            FeesCategoryB = college.FeesCategoryB,
            FeesCategoryC = college.FeesCategoryC,
            FeesPerHour = college.FeesPerHour,
            MinimumHoursPerSemester = college.MinimumHoursPerSemester,
            AdditionalFees = college.AdditionalFees
        };

        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCollege(BLL.ModelVM.UpdateCollegeDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var college = await _universityService.UpdateCollegeAsync(dto);
                return RedirectToAction("CollegeDetails", new { id = college.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
            }
        }
        return View(dto);
    }
}





