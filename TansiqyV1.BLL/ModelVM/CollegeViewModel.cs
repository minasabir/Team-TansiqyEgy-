using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class CollegeViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int UniversityId { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal? Fees { get; set; }
    public decimal? LastYearCoordination { get; set; }
    
    // مصروفات بفئات
    public decimal? FeesCategoryA { get; set; }
    public decimal? FeesCategoryB { get; set; }
    public decimal? FeesCategoryC { get; set; }
    
    // مصروفات بالساعة
    public decimal? FeesPerHour { get; set; }
    public int? MinimumHoursPerSemester { get; set; }
    public decimal? AdditionalFees { get; set; }
    
    public int DepartmentsCount { get; set; }
    public List<DepartmentViewModel> Departments { get; set; } = new();
    public UniversityBasicViewModel? University { get; set; }
}





