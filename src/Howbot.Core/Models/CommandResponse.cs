using System;
using Discord;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Models;

public class CommandResponse
{
  public bool IsSuccessful { get; private init; }
  public bool IsEphemeral { get; private set; }
  public string Message { get; private init; }
  public Exception Exception { get; private init; }
  public IEmbed Embed { get; private init; }
  public LavalinkTrack LavalinkTrack { get; private init; }
  
  public static CommandResponse Create(bool isSuccessful, string message = "", bool isEphemeral = false, Exception exception = null, IEmbed embed = null, LavalinkTrack lavalinkTrack = null)
  {
    return new CommandResponse
    {
      IsSuccessful = isSuccessful,
      IsEphemeral = isEphemeral,
      Message = message,
      Exception = exception,
      Embed = embed,
      LavalinkTrack = lavalinkTrack
    };
  }
}
