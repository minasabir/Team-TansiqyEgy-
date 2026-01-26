using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum UniversityType
{
    [Description("جامعات حكومية")]
    Governmental = 1,
    
    [Description("جامعات خاصة")]
    Private = 2,
    
    [Description("جامعات أهلية")]
    National = 3,
    
    [Description("معاهد عالية")]
    HigherInstitute = 4,
    
    [Description("جامعات أجنبية")]
    Foreign = 5,
    
    [Description("جامعات تكنولوجية")]
    Technological = 6
}
