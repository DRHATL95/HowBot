using System;
using Howbot.Core.Models;
using Serilog;

namespace Howbot.Core.Helpers;

/// <summary>
/// Class of static helpers used to handle <seealso cref="Discord.Interactions.InteractionModuleBase"/> execution.
/// </summary>
public static class ModuleHelper
{
  /// <summary>
  /// Helper function to handle Module Command failed. Should handle exceptions or reponses returned from Services.
  /// </summary>
  /// <param name="commandResponse"></param>
  public static void HandleCommandFailed(CommandResponse commandResponse)
  {
    ArgumentNullException.ThrowIfNull(commandResponse, nameof(commandResponse));

    if (commandResponse.Exception != null)
    {
      throw commandResponse.Exception;
    }

    if (!string.IsNullOrEmpty(commandResponse.Message))
    {
      Log.Logger.Error(commandResponse.Message);
    }
  }

  /// <summary>
  /// Helper function to check if Command Module parameters are valid.
  /// </summary>
  /// <param name="args"></param>
  /// <returns></returns>
  public static bool CheckValidCommandParameter(params object[] args)
  {
    foreach (var arg in args)
    {
      switch (arg)
      {
        case int intArg when intArg <= 0:
        case string stringArg when string.IsNullOrEmpty(stringArg):
          return false;

        case TimeSpan timeSpanArg when timeSpanArg == default:
          return false;

        default:
          return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Helper function used to convert hours,minutes and seconds to a <see cref="TimeSpan"/>.
  /// </summary>
  /// <param name="hours"></param>
  /// <param name="minutes"></param>
  /// <param name="seconds"></param>
  /// <returns></returns>
  public static TimeSpan ConvertToTimeSpan(int hours, int minutes, int seconds)
  {
    if (hours == 0 && minutes == 0 && seconds == 0) return new TimeSpan();
    if (hours < 0 || minutes < 0 || seconds < 0) return new TimeSpan();

    return new TimeSpan(hours, minutes, seconds);
  }
}
