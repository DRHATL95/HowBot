using Discord;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Models.Players;

public sealed record HowbotPlayerOptions() : QueuedLavalinkPlayerOptions
{
  public HowbotPlayerOptions(ITextChannel textChannel, IUser lastRequestedBy) : this()
  {
    TextChannel = textChannel;
    LastRequestedBy = lastRequestedBy;
  }

  public ITextChannel? TextChannel { get; }
  public IUser? LastRequestedBy { get; }
}
