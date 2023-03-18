﻿using System;
using Discord;
using JetBrains.Annotations;
using Victoria.Player;

namespace Howbot.Core.Entities;

public class CommandResponse : BaseEntity
{
  public bool Success { get; }
  
  public string Message { get; }

  [CanBeNull] public Exception Exception { get; }
  
  [CanBeNull] public IEmbed Embed { get; }
  
  [CanBeNull] public Player<LavaTrack> LavaPlayer { get; init; }

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

  public static CommandResponse CommandSuccessful() => new(true);
  
  public static CommandResponse CommandSuccessful(string message) => new(true, message);

  public static CommandResponse CommandSuccessful(Player<LavaTrack> lavaPlayer) => new(lavaPlayer);
  
  public static CommandResponse CommandSuccessful(IEmbed embed) => new(embed);

  public static CommandResponse CommandNotSuccessful() => new(false);

  public static CommandResponse CommandNotSuccessful(string message) => new(false, message);

  public static CommandResponse CommandNotSuccessful(Exception exception) => new(exception);
}
