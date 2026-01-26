using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.ModelVM;

public class BranchViewModel
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Location { get; set; }
    public int Governorate { get; set; } // رقم الـ enum للتوافق مع الـ Frontend
    public string GovernorateAr { get; set; } = string.Empty;
}





