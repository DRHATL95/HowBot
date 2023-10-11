﻿using System;
using Discord;
using JetBrains.Annotations;
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
  [CanBeNull] public string Message { get; private init; }
  [CanBeNull] public Exception Exception { get; private init; }
  [CanBeNull] public IEmbed Embed { get; private init; }
  [CanBeNull] public LavalinkTrack LavalinkTrack { get; private init; }

  [NotNull]
  public static CommandResponse CommandSuccessful()
  {
    return new CommandResponse { IsSuccessful = true };
  }

  [NotNull]
  public static CommandResponse CommandSuccessful([NotNull] string message)
  {
    return new CommandResponse { IsSuccessful = true, Message = message };
  }

  [NotNull]
  public static CommandResponse CommandSuccessful([NotNull] LavalinkTrack lavalinkTrack)
  {
    return new CommandResponse { IsSuccessful = true, LavalinkTrack = lavalinkTrack };
  }

  [NotNull]
  public static CommandResponse CommandSuccessful([NotNull] IEmbed embed)
  {
    return new CommandResponse { IsSuccessful = true, Embed = embed };
  }

  [NotNull]
  public static CommandResponse CommandNotSuccessful()
  {
    return new CommandResponse { IsSuccessful = false };
  }

  [NotNull]
  public static CommandResponse CommandNotSuccessful([NotNull] string message)
  {
    return new CommandResponse { IsSuccessful = false, Message = message };
  }

  [NotNull]
  public static CommandResponse CommandNotSuccessful([NotNull] Exception exception)
  {
    return new CommandResponse { IsSuccessful = false, Exception = exception };
  }
}
