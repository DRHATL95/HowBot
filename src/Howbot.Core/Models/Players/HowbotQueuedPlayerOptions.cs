using Discord;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Models.Players;
public sealed record HowbotPlayerOptions() : QueuedLavalinkPlayerOptions
{
  public ITextChannel TextChannel { get; }
  
  public HowbotPlayerOptions(ITextChannel textChannel) : this()
  {
    this.TextChannel = textChannel;
  }
}
