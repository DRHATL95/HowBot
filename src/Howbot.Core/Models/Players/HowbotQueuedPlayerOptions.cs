using Discord;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Models.Players;
public sealed record HowbotPlayerOptions() : QueuedLavalinkPlayerOptions
{
  public ITextChannel TextChannel { get; }
  public IUser LastRequestedBy { get; }
  
  public HowbotPlayerOptions(ITextChannel textChannel, IUser lastRequestedBy) : this()
  {
    this.TextChannel = textChannel;
    this.LastRequestedBy = lastRequestedBy;
  }
}
