using System;
using JetBrains.Annotations;
using Victoria.Player;

namespace Howbot.Core.Entities;

public class CommandResponse : BaseEntity
{
  public bool Success { get; set; }
  
  [CanBeNull] public Exception Exception { get; set; }
  
  public string Message { get; set; }
  
  [CanBeNull] public LavaPlayer<LavaTrack> LavaPlayer { get; set; }

  public CommandResponse()
  {
    Message = string.Empty;
    Success = false;
    Exception = null;
  }

  private CommandResponse(bool success)
  {
    Success = success;
    Message = String.Empty;
    Exception = null;
  }

  private CommandResponse(LavaPlayer lavaPlayer)
  {
    Success = true;
    LavaPlayer = lavaPlayer;
  }

  private CommandResponse(LavaPlayer<LavaTrack> lavaPlayer)
  {
    Success = true;
    LavaPlayer = lavaPlayer;
  }

  private CommandResponse(string message)
  {
    Message = message;
    Success = false;
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

  public static CommandResponse CommandSuccessful() => new(true);

  public static CommandResponse CommandSuccessful(LavaPlayer<LavaTrack> lavaPlayer) => new(lavaPlayer);
  
  public static CommandResponse CommandSuccessful(LavaPlayer lavaPlayer) => new(lavaPlayer);

  public static CommandResponse CommandNotSuccessful() => new(false);

  public static CommandResponse CommandNotSuccessful(string message) => new(false, message);

  public static CommandResponse CommandNotSuccessful(Exception exception) => new(exception);
}
