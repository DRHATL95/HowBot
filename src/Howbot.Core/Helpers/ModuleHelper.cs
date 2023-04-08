using System;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Serilog;

namespace Howbot.Core.Helpers;

public static class ModuleHelper
{
  public static void HandleCommandFailed(CommandResponse commandResponse)
  {
    ArgumentNullException.ThrowIfNull(commandResponse, nameof(commandResponse));

    if (!string.IsNullOrEmpty(commandResponse.CommandName))
    {
      Log.Logger.Information("Command has failed.");
      return;
    }

    Log.Logger.Information("{CommandName} has failed", commandResponse.CommandName);

    if (commandResponse.Exception != null)
    {
      throw commandResponse.Exception;
    }

    if (!string.IsNullOrEmpty(commandResponse.Message))
    {
      Log.Logger.Error(commandResponse.Message);
    }
  }

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
  
  public static TimeSpan ConvertToTimeSpan(int hours, int minutes, int seconds)
  {
    if (hours == 0 && minutes == 0 && seconds == 0) return new TimeSpan();
    if (hours < 0 || minutes < 0 || seconds < 0) return new TimeSpan();

    return new TimeSpan(hours, minutes, seconds);
  }
}
