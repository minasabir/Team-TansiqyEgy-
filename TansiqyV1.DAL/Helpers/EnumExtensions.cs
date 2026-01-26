using System.ComponentModel;
using System.Reflection;

namespace TansiqyV1.DAL.Helpers;

public static class EnumExtensions
{
    /// <summary>
    /// Gets the Description attribute value of an enum value.
    /// If no Description attribute is found, returns the enum name as string.
    /// </summary>
    /// <param name="enumValue">The enum value</param>
    /// <returns>The description from [Description] attribute, or the enum name if not found</returns>
    public static string GetDescription(this Enum enumValue)
    {
        if (enumValue == null)
            return string.Empty;
            
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString(), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        if (fieldInfo == null)
            return enumValue.ToString();
        
        var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
        
        return descriptionAttribute?.Description ?? enumValue.ToString();
    }
}

