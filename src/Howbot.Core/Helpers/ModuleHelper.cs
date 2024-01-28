using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Howbot.Core.Models;
using Serilog;
using CommandException = Howbot.Core.Models.Exceptions.CommandException;

namespace Howbot.Core.Helpers;

/// <summary>
/// Class of static helpers used to handle <seealso cref="Discord.Interactions.InteractionModuleBase"/> execution.
/// </summary>
public static class ModuleHelper
{
  /// <summary>
  /// Helper function to handle Module Command failed.
  /// Should handle exceptions or responses returned from Services.
  /// </summary>
  /// <param name="commandResponse"></param>
  /// <exception cref="Models.Exceptions.CommandException"></exception>
  public static void HandleCommandFailed(CommandResponse commandResponse)
  {
    ArgumentNullException.ThrowIfNull(commandResponse, nameof(commandResponse));

    if (commandResponse.Exception != null)
    {
      throw new CommandException(commandResponse.Exception.Message, commandResponse.Exception.InnerException);
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
        case int intArg when intArg < 0:
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

  public static async Task HandleCommandResponseAsync(CommandResponse response, SocketInteractionContext context,
    bool shouldDelete = false)
  {
    if (shouldDelete)
    {
      var originalResponse = await context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);
      await originalResponse.DeleteAsync().ConfigureAwait(false);
    }

    if (response.IsSuccessful)
    {
      if (response.Embed != null)
      {
        await context.Interaction.FollowupAsync(embed: response.Embed as Embed).ConfigureAwait(false);
        return;
      }

      if (!string.IsNullOrEmpty(response.Message))
      {
        await context.Interaction.FollowupAsync(response.Message).ConfigureAwait(false);
      }
    }
    else
    {
      if (response.Exception != null)
      {
        throw new CommandException(response.Exception.Message, response.Exception.InnerException);
      }

      if (!string.IsNullOrEmpty(response.Message))
      {
        await context.Interaction.FollowupAsync(response.Message).ConfigureAwait(false);
      }
    }
  }
  
  public static readonly Dictionary<string, string> CommandExampleDictionary = new Dictionary<string, string>()
  {
    { Constants.Commands.PingCommandName, "/ping" },
    { Constants.Commands.HelpCommandName, "/help" },
    { Constants.Commands.JoinCommandName, "/join" },
    { Constants.Commands.LeaveCommandName, "/leave" },
    { Constants.Commands.PlayCommandName, "/play https://www.youtube.com/watch?v=dQw4w9WgXcQ" },
    // { Constants.Commands.StopCommandName, "/stop" },
    { Constants.Commands.PauseCommandName, "/pause" },
    { Constants.Commands.ResumeCommandName, "/resume" },
    { Constants.Commands.SkipCommandName, "/skip" },
    // { Constants.Commands.QueueCommandName, "/queue" },
    // { Constants.Commands.ClearCommandName, "/clear" },
    { Constants.Commands.SeekCommandName, "/seek 1:30" },
    { Constants.Commands.VolumeCommandName, "/volume 50" },
    { Constants.Commands.ShuffleCommandName, "/shuffle" },
    { Constants.Commands.NowPlayingCommandName, "/nowplaying" },
  };
}
