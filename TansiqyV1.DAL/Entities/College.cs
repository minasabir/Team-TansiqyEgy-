using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class College : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int UniversityId { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal? Fees { get; set; } // مصروفات الكلية (قد تختلف عن الجامعة)
    public decimal? LastYearCoordination { get; set; } // تنسيق الكلية
    
    // مصروفات بفئات (للدعم الجامعات الأهلية والأجنبية)
    public decimal? FeesCategoryA { get; set; } // فئة أ / Category A
    public decimal? FeesCategoryB { get; set; } // فئة ب / Category B
    public decimal? FeesCategoryC { get; set; } // فئة ج / Category C
    
    // مصروفات بالساعة (للدعم المعاهد العالية)
    public decimal? FeesPerHour { get; set; } // مصروفات الساعة الواحدة
    public int? MinimumHoursPerSemester { get; set; } // الحد الأدنى للساعات في الفصل الدراسي
    public decimal? AdditionalFees { get; set; } // مصروفات إضافية
    
    // Navigation Properties
    public virtual University University { get; set; } = null!;
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}





