using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum UserRole
{
    [Description("مسؤول")]
    Admin = 1,
    
    [Description("طالب")]
    Student = 2
}
