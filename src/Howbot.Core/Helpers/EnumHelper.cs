using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Howbot.Core.Helpers;
public static class EnumHelper
{
  public static T GetValueFromDescription<T>([CanBeNull] string description) where T : Enum
  {
    foreach (var field in typeof(T).GetFields())
    {
      if (Attribute.GetCustomAttribute(field,
            typeof(DescriptionAttribute)) is not DescriptionAttribute attribute)
      {
        continue;
      }

      if (attribute.Description.ToLower() == description?.ToLower())
      {
        return (T)field.GetValue(null);
      }

      if (field.Name.ToLower() == description?.ToLower())
      {
        return (T)field.GetValue(null);
      }
    }

    throw new ArgumentException("Not found.", nameof(description));
  }

  public static TEnum ConvertToEnum<TEnum>(string value)
  {
    try
    {
      return (TEnum)Enum.Parse(typeof(TEnum), value, true);
    }
    catch (ArgumentException)
    {
      // Handle the case when the string does not represent a valid enum value
      throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name} value");
    }
  }
}
