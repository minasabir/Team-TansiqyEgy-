using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class CreateCollegeDto
{
    [Required(ErrorMessage = "اسم الكلية بالعربي مطلوب")]
    [StringLength(200, ErrorMessage = "اسم الكلية لا يجب أن يتجاوز 200 حرف")]
    public string NameAr { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "اسم الكلية بالإنجليزي لا يجب أن يتجاوز 200 حرف")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "معرف الجامعة مطلوب")]
    public int UniversityId { get; set; }

    [Url(ErrorMessage = "الموقع الرسمي يجب أن يكون رابط صحيح")]
    [StringLength(500, ErrorMessage = "الموقع الرسمي لا يجب أن يتجاوز 500 حرف")]
    public string? OfficialWebsite { get; set; }

    [StringLength(500, ErrorMessage = "الموقع لا يجب أن يتجاوز 500 حرف")]
    public string? Location { get; set; }

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "المصاريف يجب أن تكون قيمة موجبة")]
    public decimal? Fees { get; set; }

    [Range(0, 100, ErrorMessage = "التنسيق يجب أن يكون بين 0 و 100")]
    public decimal? LastYearCoordination { get; set; }

    // مصروفات بفئات (للجامعات الأهلية والأجنبية)
    [Range(0, double.MaxValue, ErrorMessage = "مصروفات الفئة أ يجب أن تكون قيمة موجبة")]
    public decimal? FeesCategoryA { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "مصروفات الفئة ب يجب أن تكون قيمة موجبة")]
    public decimal? FeesCategoryB { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "مصروفات الفئة ج يجب أن تكون قيمة موجبة")]
    public decimal? FeesCategoryC { get; set; }

    // مصروفات بالساعة (للمعاهد العالية)
    [Range(0, double.MaxValue, ErrorMessage = "مصروفات الساعة يجب أن تكون قيمة موجبة")]
    public decimal? FeesPerHour { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الحد الأدنى للساعات يجب أن يكون قيمة موجبة")]
    public int? MinimumHoursPerSemester { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "المصاريف الإضافية يجب أن تكون قيمة موجبة")]
    public decimal? AdditionalFees { get; set; }

    // للتخصصات (اختياري)
    public List<CreateDepartmentDto>? Departments { get; set; }
}








