using Discord.Interactions;

namespace Howbot.Application.Interfaces.Discord;

public interface IInteractionService
{
  Task Initialize();
  Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId);
  Task RegisterCommandsGloballyAsync();
  Task<IResult> ExecuteCommandAsync(SocketInteractionContext context);
}
