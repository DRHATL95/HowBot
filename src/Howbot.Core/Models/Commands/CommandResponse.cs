#nullable enable
using System;
using Discord;
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
  public CommandResponseMetadata Metadata { get; private init; }

  public static CommandResponse Create(bool isSuccessful, string? message = null, Exception? exception = null,
    IEmbed? embed = null, HowbotPlayer? player = null, LavalinkTrack? lavalinkTrack = null, CommandResponseMetadata metadata = default)
  {
    return new CommandResponse
    {
      IsSuccessful = isSuccessful,
      Message = message,
      Exception = exception,
      Embed = embed,
      Player = player,
      LavalinkTrack = lavalinkTrack,
      Metadata = metadata
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
      LavalinkTrack = parameters.LavalinkTrack,
      Metadata = parameters.Metadata
    };
  }

  public override string ToString()
  {
    return !string.IsNullOrEmpty(this.Message) 
      ? this.Message 
      : this.Embed?.ToString() ?? string.Empty;
  }
}

public struct CommandResponseMetadata
{
  public CommandSource Source { get; set; }
  public ulong RequestedById { get; set; }
  public DateTime ResponseDateTime { get; set; }
}

public struct CreateCommandResponseParameters
{
  public bool IsSuccessful { get; set; }
  public string? Message { get; set; }
  public Exception? Exception { get; set; }
  public IEmbed? Embed { get; set; }
  public HowbotPlayer? Player { get; set; }
  public LavalinkTrack? LavalinkTrack { get; set; }
  public CommandResponseMetadata Metadata { get; set; }
}
