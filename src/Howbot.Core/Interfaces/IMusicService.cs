using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IMusicService
{
  public Task<CommandResponse> JoinVoiceCommandAsync(IGuildUser user, ITextChannel textChannel);
}
