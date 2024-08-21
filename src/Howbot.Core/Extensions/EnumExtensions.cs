using System.ComponentModel;
using System.Reflection;

namespace Howbot.Core.Extensions;

public static class EnumExtensions
{
  public static string GetDisplayName(this Enum enumValue)
  {
    var type = enumValue.GetType();
    var enumName = enumValue.ToString();

    if (string.IsNullOrEmpty(enumName))
    {
      return string.Empty;
    }

    var memberInfo = type.GetMember(enumName).FirstOrDefault();
    if (memberInfo == null)
    {
      return enumName;
    }

    var displayNameAttribute = memberInfo
      .GetCustomAttribute<DisplayNameAttribute>();

    return displayNameAttribute != null
      ? displayNameAttribute.DisplayName
      :
      // If DisplayNameAttribute is not found, return the enum name itself
      enumName;
  }
}
