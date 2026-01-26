using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class UpdateUniversityDto
{
    [Required(ErrorMessage = "معرف الجامعة مطلوب")]
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الجامعة بالعربي مطلوب")]
    [StringLength(200, ErrorMessage = "اسم الجامعة لا يجب أن يتجاوز 200 حرف")]
    public string NameAr { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "اسم الجامعة بالإنجليزي لا يجب أن يتجاوز 200 حرف")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "نوع الجامعة مطلوب")]
    public UniversityType Type { get; set; }

    [Url(ErrorMessage = "الموقع الرسمي يجب أن يكون رابط صحيح")]
    [StringLength(500, ErrorMessage = "الموقع الرسمي لا يجب أن يتجاوز 500 حرف")]
    public string? OfficialWebsite { get; set; }

    [StringLength(500, ErrorMessage = "الموقع لا يجب أن يتجاوز 500 حرف")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    public Governorate Governorate { get; set; }

    [Range(0, 100, ErrorMessage = "التنسيق يجب أن يكون بين 0 و 100")]
    public decimal? LastYearCoordination { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "المصاريف يجب أن تكون قيمة موجبة")]
    public decimal? Fees { get; set; }

    public string? InformationSources { get; set; }
    public string? Description { get; set; }
}








