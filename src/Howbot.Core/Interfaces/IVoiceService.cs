using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Howbot.Core.Models;

namespace Howbot.Core.Interfaces;

public interface IVoiceService : IServiceBase
{
  public Task<CommandResponse> JoinVoiceAsync(IGuildUser user, ITextChannel textChannel);
  public Task<CommandResponse> LeaveVoiceChannelAsync(IGuildUser user);
}
