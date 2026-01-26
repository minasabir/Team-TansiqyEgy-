using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "اسم التخصص بالعربي مطلوب")]
    [StringLength(200, ErrorMessage = "اسم التخصص لا يجب أن يتجاوز 200 حرف")]
    public string NameAr { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "اسم التخصص بالإنجليزي لا يجب أن يتجاوز 200 حرف")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "معرف الكلية مطلوب")]
    public int CollegeId { get; set; }

    public string? Description { get; set; }

    public StudyType? StudyType { get; set; }
}








