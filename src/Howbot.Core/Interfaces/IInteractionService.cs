using System.Threading.Tasks;
using Discord.Interactions;

namespace Howbot.Core.Interfaces;

public interface IInteractionService
{
  public void Initialize();
  Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId);
  Task RegisterCommandsGloballyAsync();
  Task<IResult> ExecuteCommandAsync(SocketInteractionContext context);
}
