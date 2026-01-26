using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class DepartmentViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int? StudyType { get; set; } // رقم الـ enum للتوافق مع الـ Frontend
    public string? StudyTypeAr { get; set; }
    public string? Description { get; set; }
}





