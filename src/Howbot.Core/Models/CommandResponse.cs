using System;
using Discord;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Models;

public class CommandResponse
{
  private CommandResponse()
  {
    IsSuccessful = false;
    Message = string.Empty;
    Exception = null;
    Embed = null;
    LavalinkTrack = null;
  }

  public bool IsSuccessful { get; private init; }
  public bool IsEphemeral { get; private set; }
  public string Message { get; private init; }
  public Exception Exception { get; private init; }
  public IEmbed Embed { get; private init; }
  public LavalinkTrack LavalinkTrack { get; private init; }

  public static CommandResponse CommandSuccessful()
  {
    return new CommandResponse { IsSuccessful = true, IsEphemeral = false };
  }

  public static CommandResponse CommandSuccessful(string message)
  {
    return new CommandResponse { IsSuccessful = true, Message = message };
  }

  public static CommandResponse CommandSuccessful(string message, bool isEphemeral)
  {
    return new CommandResponse { IsSuccessful = true, Message = message, IsEphemeral = isEphemeral };
  }

  public static CommandResponse CommandSuccessful(LavalinkTrack lavalinkTrack)
  {
    return new CommandResponse { IsSuccessful = true, LavalinkTrack = lavalinkTrack };
  }

  public static CommandResponse CommandSuccessful(LavalinkTrack lavalinkTrack, bool isEphemeral)
  {
    return new CommandResponse { IsSuccessful = true, LavalinkTrack = lavalinkTrack, IsEphemeral = isEphemeral };
  }

  public static CommandResponse CommandSuccessful(IEmbed embed)
  {
    return new CommandResponse { IsSuccessful = true, Embed = embed };
  }

  public static CommandResponse CommandNotSuccessful()
  {
    return new CommandResponse { IsSuccessful = false, IsEphemeral = false };
  }

  public static CommandResponse CommandNotSuccessful(string message)
  {
    return new CommandResponse { IsSuccessful = false, Message = message };
  }

  public static CommandResponse CommandNotSuccessful(string message, bool isEphemeral)
  {
    return new CommandResponse { IsSuccessful = false, Message = message, IsEphemeral = isEphemeral };
  }

  public static CommandResponse CommandNotSuccessful(Exception exception)
  {
    return new CommandResponse { IsSuccessful = false, Exception = exception };
  }

  public static CommandResponse CommandNotSuccessful(Exception exception, bool isEphemeral)
  {
    return new CommandResponse { IsSuccessful = false, Exception = exception, IsEphemeral = isEphemeral };
  }

  public CommandResponse EnableEphemeral()
  {
    IsEphemeral = true;
    return this;
  }

  public CommandResponse DisableEphemeral()
  {
    IsEphemeral = false;
    return this;
  }
}
