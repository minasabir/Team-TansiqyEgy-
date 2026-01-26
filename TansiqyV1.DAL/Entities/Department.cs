using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class Department : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int CollegeId { get; set; }
    public string? Description { get; set; }
    public StudyType? StudyType { get; set; } // نوع الدراسة المطلوب
    
    // Navigation Properties
    public virtual College College { get; set; } = null!;
}





