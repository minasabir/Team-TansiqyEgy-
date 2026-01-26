using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class University : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public UniversityType Type { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public Governorate Governorate { get; set; }
    public decimal? LastYearCoordination { get; set; } // تنسيق السنة الفائتة
    public decimal? Fees { get; set; } // المصاريف
    public string? InformationSources { get; set; } // مصادر المعلومات
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<College> Colleges { get; set; } = new List<College>();
    public virtual ICollection<UniversityBranch> Branches { get; set; } = new List<UniversityBranch>();
}





