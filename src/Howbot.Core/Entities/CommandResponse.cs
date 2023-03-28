using System;
using Discord;
using JetBrains.Annotations;
using Victoria.Player;

namespace Howbot.Core.Entities;

public class CommandResponse : BaseEntity
{
  private CommandResponse()
  {
    Message = string.Empty;
    Success = false;
    Exception = null;
    Embed = null;
  }

  private CommandResponse(bool success)
  {
    Success = success;
    Message = String.Empty;
    Exception = null;
  }

  private CommandResponse(Player<LavaTrack> lavaPlayer)
  {
    Success = true;
    LavaPlayer = lavaPlayer;
  }

  private CommandResponse(IEmbed embed)
  {
    Success = true;
    Embed = embed;
  }

  private CommandResponse(string message)
  {
    Message = message;
    Success = true;
    Exception = null;
  }

  private CommandResponse(bool success, string message)
  {
    Success = success;
    Message = message;
    Exception = null;
  }

  private CommandResponse(Exception exception)
  {
    Success = false;
    Exception = exception;
  }

  public bool Success { get; }

  public string Message { get; }

  [CanBeNull] public Exception Exception { get; }

  [CanBeNull] public IEmbed Embed { get; }

  [CanBeNull] public Player<LavaTrack> LavaPlayer { get; init; }

  public static CommandResponse CommandSuccessful()
  {
    return new(true);
  }

  public static CommandResponse CommandSuccessful(string message)
  {
    return new(true, message);
  }

  public static CommandResponse CommandSuccessful(Player<LavaTrack> lavaPlayer)
  {
    return new(lavaPlayer);
  }

  public static CommandResponse CommandSuccessful(IEmbed embed)
  {
    return new(embed);
  }

  public static CommandResponse CommandNotSuccessful()
  {
    return new(false);
  }

  public static CommandResponse CommandNotSuccessful(string message)
  {
    return new(false, message);
  }

  public static CommandResponse CommandNotSuccessful(Exception exception)
  {
    return new(exception);
  }
}
