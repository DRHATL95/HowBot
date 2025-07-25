using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Howbot.Application.Constants;
using Howbot.Application.Models;
using Howbot.Application.Models.Discord.Commands;
using Howbot.Application.Models.Lavalink;
using CommandException = Howbot.Application.Exceptions.CommandException;

namespace Howbot.Application.Helpers;

public static class ModuleHelper
{
  public static readonly Dictionary<string, List<string>> CommandExampleDictionary = new()
  {
    { CommandMetadata.PingCommandName, ["/ping"] },
    { CommandMetadata.HelpCommandName, ["/help", "/help ping"] },
    { CommandMetadata.JoinCommandName, ["/join"] },
    { CommandMetadata.LeaveCommandName, ["/leave"] },
    {
      CommandMetadata.PlayCommandName,
      ["/play https://www.youtube.com/watch?v=dQw4w9WgXcQ", "/play my favorite song"]
    },
    { CommandMetadata.PauseCommandName, ["/pause"] },
    { CommandMetadata.ResumeCommandName, ["/resume"] },
    { CommandMetadata.SkipCommandName, ["/skip"] },
    { CommandMetadata.QueueCommandName, ["/queue"] },
    { CommandMetadata.ClearCommandName, ["/clear"] },
    // { Constants.Commands.LoopCommandName, ["/loop"] },
    { CommandMetadata.SeekCommandName, ["/seek 1:30", "/seek 0 1 30"] },
    { CommandMetadata.VolumeCommandName, ["/volume 50"] },
    { CommandMetadata.ShuffleCommandName, ["/shuffle"] },
    { CommandMetadata.NowPlayingCommandName, ["/nowplaying"] },
    { CommandMetadata.BanCommandName, ["/ban @user"] },
    { CommandMetadata.KickCommandName, ["/kick @user"] },
    { CommandMetadata.MuteCommandName, ["/mute @user"] },
    { CommandMetadata.UnmuteCommandName, ["/unmute @user"] },
    { CommandMetadata.SlowmodeCommandName, ["/slowmode 5"] },
    { CommandMetadata.LockCommandName, ["/lock"] },
    { CommandMetadata.UnlockCommandName, ["/unlock"] },
    { CommandMetadata.PurgeCommandName, ["/purge 5"] },
    { CommandMetadata.SayCommandName, ["/say Hello, World!"] }
  };

  public static void HandleCommandFailed(CommandResponse commandResponse)
  {
    Guard.Against.Null(commandResponse, nameof(commandResponse));

    if (!string.IsNullOrEmpty(commandResponse.Message))
    {
      throw new CommandException(commandResponse.Message);
    }

    if (commandResponse.Exception == null)
    {
      throw new CommandException("An unknown error occurred. Please try again.");
    }

    if (commandResponse.Exception.InnerException != null)
    {
      throw new CommandException(commandResponse.Exception.Message, commandResponse.Exception.InnerException);
    }

    throw new CommandException(commandResponse.Exception.Message);
  }

  public static void HandleCommandFailed(MusicCommandResult commandResult)
  {
    Guard.Against.Null(commandResult, nameof(commandResult));

    if (!string.IsNullOrEmpty(commandResult.Message))
    {
      throw new CommandException(commandResult.Message);
    }

    if (commandResult.Exception == null)
    {
      throw new CommandException("An unknown error occurred. Please try again.");
    }

    if (commandResult.Exception.InnerException != null)
    {
      throw new CommandException(commandResult.Exception.Message, commandResult.Exception.InnerException);
    }

    throw new CommandException(commandResult.Exception.Message);
  }

  public static bool CheckValidCommandParameter(params object[] args)
  {
    foreach (var arg in args)
    {
      switch (arg)
      {
        case int and < 0:
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

      return;
    }

    if (response.IsSuccessful)
    {
      if (response.Embed != null)
      {
        await context.Interaction.FollowupAsync(embed: response.Embed as Embed);
        return;
      }
    }
    else
    {
      if (response.Exception != null)
      {
        throw new CommandException(response.Exception.Message, response.Exception?.InnerException ?? new Exception());
      }
    }

    if (!string.IsNullOrEmpty(response.Message))
    {
      await context.Interaction.FollowupAsync(response.Message);
    }
  }
}
