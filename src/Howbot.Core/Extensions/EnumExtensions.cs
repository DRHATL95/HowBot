using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Howbot.Core.Extensions;

public static class EnumExtensions
{
  public static string GetDisplayName(this Enum enumValue)
  {
    var type = enumValue.GetType();
    var enumName = enumValue.ToString(); // Convert to lowercase

    var memberInfo = type.GetMember(enumName).FirstOrDefault();
    if (memberInfo != null)
    {
      var displayNameAttribute = memberInfo
        .GetCustomAttribute<DisplayNameAttribute>();

      if (displayNameAttribute != null)
      {
        return displayNameAttribute.DisplayName;
      }
    }

    // If DisplayNameAttribute is not found, return the enum name itself
    return enumName;
  }
}
