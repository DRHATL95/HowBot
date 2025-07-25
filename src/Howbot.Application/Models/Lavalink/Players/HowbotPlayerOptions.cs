using Discord;
using Lavalink4NET.Players.Queued;

namespace Howbot.Application.Models.Lavalink.Players;

public sealed record HowbotPlayerOptions() : QueuedLavalinkPlayerOptions
{
  public ulong? TextChannelId { get; set; }

  public bool IsAutoPlayEnabled { get; init; }
}
