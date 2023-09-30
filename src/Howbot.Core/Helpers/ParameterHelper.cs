using System;

namespace Howbot.Core.Helpers;

public static class ParameterHelper
{
  // Create a method to ensure that a parameter is not null and above 0 if number type
  public static void EnsureNotNullAndZeroOrGreater<T>(T parameter, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(parameter, parameterName);

    switch (parameter)
    {
      case <= 0:
        throw new ArgumentOutOfRangeException(parameterName);
      case long and <= 0:
        throw new ArgumentOutOfRangeException(parameterName);
      case double and <= 0:
        throw new ArgumentOutOfRangeException(parameterName);
      case float and <= 0:
        throw new ArgumentOutOfRangeException(parameterName);
      case decimal and <= 0:
        throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
