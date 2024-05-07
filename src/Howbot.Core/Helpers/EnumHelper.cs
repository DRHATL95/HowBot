using Ardalis.GuardClauses;

namespace Howbot.Core.Helpers;

public static class EnumHelper
{
  /*public static T? GetValueFromDescription<T>(string description) where T : Enum
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
  }*/

  public static TEnum ConvertToEnum<TEnum>(string? value)
  {
    Guard.Against.NullOrEmpty(value, nameof(value));

    return (TEnum)Enum.Parse(typeof(TEnum), value, true);
  }
}
