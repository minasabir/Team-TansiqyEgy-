using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum StudyType
{
    [Description("علم رياضة")]
    Math = 1,
    
    [Description("علم علوم")]
    Science = 2,
    
    [Description("أدبي")]
    Literary = 3,
    
    [Description("صنايع")]
    Industrial = 4,
    
    [Description("أمريكان")]
    American = 5
}
