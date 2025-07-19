using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Tracks;

namespace Howbot.Application.Models.Lavalink;
public class MusicCommandResult
{
  public bool IsSuccessful { get; init; }
  public string? Message { get; init; }
  public Exception? Exception { get; init; }
  public LavalinkTrack? Track { get; init; }
  public object? Data { get; init; }

  public static MusicCommandResult Success(string? message = null, LavalinkTrack? track = null, object? data = null)
    => new() { IsSuccessful = true, Message = message, Track = track, Data = data };

  public static MusicCommandResult Failure(string? message = null, Exception? exception = null, object? data = null)
    => new() { IsSuccessful = false, Message = message, Exception = exception, Data = data };

  public override string ToString() => Message ?? Track?.ToString() ?? string.Empty;
}
