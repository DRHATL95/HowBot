using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Howbot.Core.Extensions;
public static class EnumExtensions
{
  public static string GetDisplayName(this Enum enumValue)
  {
    Type type = enumValue.GetType();
    string enumName = enumValue.ToString(); // Convert to lowercase

    MemberInfo memberInfo = type.GetMember(enumName).FirstOrDefault();
    if (memberInfo != null)
    {
      DisplayNameAttribute displayNameAttribute = memberInfo
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
