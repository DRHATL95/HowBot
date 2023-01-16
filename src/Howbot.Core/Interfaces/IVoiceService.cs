using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IVoiceService
{
  public Task<CommandResponse> JoinVoiceCommandAsync(IGuildUser user, ITextChannel textChannel);
}
