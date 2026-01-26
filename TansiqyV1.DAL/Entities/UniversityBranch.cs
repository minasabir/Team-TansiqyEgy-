using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class UniversityBranch : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int UniversityId { get; set; }
    public string? Location { get; set; }
    public Governorate Governorate { get; set; }
    
    // Navigation Properties
    public virtual University University { get; set; } = null!;
}





