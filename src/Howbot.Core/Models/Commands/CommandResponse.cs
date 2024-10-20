﻿using Discord;
using Howbot.Core.Models.Players;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Models.Commands;

public class CommandResponse
{
  public bool IsSuccessful { get; private init; }
  public string? Message { get; private init; }
  public Exception? Exception { get; private init; }
  public IEmbed? Embed { get; private init; }
  public HowbotPlayer? Player { get; private init; }
  public LavalinkTrack? LavalinkTrack { get; private init; }

  public static CommandResponse Create(bool isSuccessful, string? message = null, Exception? exception = null,
    IEmbed? embed = null, HowbotPlayer? player = null, LavalinkTrack? lavalinkTrack = null)
  {
    return new CommandResponse
    {
      IsSuccessful = isSuccessful,
      Message = message,
      Exception = exception,
      Embed = embed,
      Player = player,
      LavalinkTrack = lavalinkTrack
    };
  }

  public static CommandResponse Create(CreateCommandResponseParameters parameters)
  {
    return new CommandResponse
    {
      IsSuccessful = parameters.IsSuccessful,
      Message = parameters.Message,
      Exception = parameters.Exception,
      Embed = parameters.Embed,
      Player = parameters.Player,
      LavalinkTrack = parameters.LavalinkTrack
    };
  }

  public override string ToString()
  {
    return !string.IsNullOrEmpty(Message)
      ? Message
      : Embed?.ToString() ?? string.Empty;
  }
}

public struct CreateCommandResponseParameters
{
  public bool IsSuccessful { get; set; }
  public string? Message { get; set; }
  public Exception? Exception { get; set; }
  public IEmbed? Embed { get; set; }
  public HowbotPlayer? Player { get; set; }
  public LavalinkTrack? LavalinkTrack { get; set; }
}
