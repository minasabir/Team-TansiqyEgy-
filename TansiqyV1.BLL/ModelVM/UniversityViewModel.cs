using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class UniversityViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int Type { get; set; } // رقم الـ enum للتوافق مع الـ Frontend
    public string TypeAr { get; set; } = string.Empty;
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public int Governorate { get; set; } // رقم الـ enum للتوافق مع الـ Frontend
    public string GovernorateAr { get; set; } = string.Empty;
    public decimal? LastYearCoordination { get; set; }
    public decimal? Fees { get; set; }
    public string? InformationSources { get; set; }
    public string? Description { get; set; }
    public int CollegesCount { get; set; }
    public int BranchesCount { get; set; }
    public List<CollegeViewModel> Colleges { get; set; } = new();
    public List<BranchViewModel> Branches { get; set; } = new();
}





