using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Serilog;
using CommandException = Howbot.Core.Models.Exceptions.CommandException;

namespace Howbot.Core.Helpers;

public static class ModuleHelper
{
  public static readonly Dictionary<string, string> CommandExampleDictionary = new()
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
    { Constants.Commands.NowPlayingCommandName, "/nowplaying" }
  };

  public static void HandleCommandFailed(CommandResponse commandResponse)
  {
    Guard.Against.Null(commandResponse, nameof(commandResponse));

    if (commandResponse.Exception == null)
    {
      return;
    }

    if (commandResponse.Exception.InnerException != null)
    {
      throw new CommandException(commandResponse.Exception.Message, commandResponse.Exception.InnerException);
    }
    throw new CommandException(commandResponse.Exception.Message);
  }

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

  public static TimeSpan ConvertToTimeSpan(int hours, int minutes, int seconds)
  {
    if (hours == 0 && minutes == 0 && seconds == 0)
    {
      return new TimeSpan();
    }

    if (hours < 0 || minutes < 0 || seconds < 0)
    {
      return new TimeSpan();
    }

    return new TimeSpan(hours, minutes, seconds);
  }

  public static async Task HandleCommandResponseAsync(CommandResponse response, SocketInteractionContext context,
    bool shouldDelete = false)
  {
    if (shouldDelete)
    {
      var originalResponse = await context.Interaction.GetOriginalResponseAsync();

      await originalResponse.DeleteAsync();
    }

    if (response.IsSuccessful)
    {
      if (response.Embed != null)
      {
        await context.Interaction.FollowupAsync(embed: response.Embed as Embed);
        return;
      }

      if (!string.IsNullOrEmpty(response.Message))
      {
        await context.Interaction.FollowupAsync(response.Message);
      }
    }
    else
    {
      if (response.Exception != null)
      {
        throw new CommandException(response.Exception.Message, response.Exception?.InnerException ?? new Exception());
      }

      if (!string.IsNullOrEmpty(response.Message))
      {
        await context.Interaction.FollowupAsync(response.Message);
      }
    }
  }
}
