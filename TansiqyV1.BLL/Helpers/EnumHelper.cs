using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.Helpers;

public static class EnumHelper
{
    public static string GetUniversityTypeNameAr(UniversityType type)
    {
        return type switch
        {
            UniversityType.Governmental => "جامعات حكومية",
            UniversityType.Private => "جامعات خاصة",
            UniversityType.National => "جامعات أهلية",
            UniversityType.HigherInstitute => "معاهد عالية",
            UniversityType.Foreign => "جامعات أجنبية",
            UniversityType.Technological => "جامعات تكنولوجية",
            _ => type.ToString()
        };
    }

    public static string GetGovernorateNameAr(Governorate governorate)
    {
        return governorate switch
        {
            Governorate.Cairo => "القاهرة",
            Governorate.Alexandria => "الإسكندرية",
            Governorate.Giza => "الجيزة",
            Governorate.Sharqia => "الشرقية",
            Governorate.Dakahlia => "الدقهلية",
            Governorate.Beheira => "البحيرة",
            Governorate.Monufia => "المنوفية",
            Governorate.Gharbia => "الغربية",
            Governorate.KafrElSheikh => "كفر الشيخ",
            Governorate.Qalyubia => "القليوبية",
            Governorate.BeniSuef => "بني سويف",
            Governorate.Fayoum => "الفيوم",
            Governorate.Minya => "المنيا",
            Governorate.Asyut => "أسيوط",
            Governorate.Sohag => "سوهاج",
            Governorate.Qena => "قنا",
            Governorate.Luxor => "الأقصر",
            Governorate.Aswan => "أسوان",
            Governorate.RedSea => "البحر الأحمر",
            Governorate.NewValley => "الوادي الجديد",
            Governorate.Matruh => "مطروح",
            Governorate.NorthSinai => "شمال سيناء",
            Governorate.SouthSinai => "جنوب سيناء",
            Governorate.PortSaid => "بورسعيد",
            Governorate.Ismailia => "الإسماعيلية",
            Governorate.Suez => "السويس",
            Governorate.Damietta => "دمياط",
            _ => governorate.ToString()
        };
    }

    public static string GetStudyTypeNameAr(StudyType type)
    {
        return type switch
        {
            StudyType.Math => "علم رياضة",
            StudyType.Science => "علم علوم",
            StudyType.Literary => "أدبي",
            StudyType.Industrial => "صنايع",
            StudyType.American => "أمريكان",
            _ => type.ToString()
        };
    }
}

