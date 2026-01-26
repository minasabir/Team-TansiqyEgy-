using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class UniversityBasicViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public int Type { get; set; } // رقم الـ enum للتوافق مع الـ Frontend
    public string TypeAr { get; set; } = string.Empty;
}





