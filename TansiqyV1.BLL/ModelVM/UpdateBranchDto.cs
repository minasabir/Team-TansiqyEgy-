using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class UpdateBranchDto
{
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الفرع بالعربي مطلوب")]
    [StringLength(200, ErrorMessage = "اسم الفرع لا يجب أن يتجاوز 200 حرف")]
    public string NameAr { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "اسم الفرع بالإنجليزي لا يجب أن يتجاوز 200 حرف")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    public Governorate Governorate { get; set; }

    [StringLength(500, ErrorMessage = "الموقع لا يجب أن يتجاوز 500 حرف")]
    public string? Location { get; set; }
}








