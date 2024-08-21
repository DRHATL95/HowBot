using Discord.Interactions;

namespace Howbot.Core.Interfaces;

public interface IInteractionService
{
  Task Initialize();
  Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId);
  Task RegisterCommandsGloballyAsync();
  Task<IResult> ExecuteCommandAsync(SocketInteractionContext context);
}
