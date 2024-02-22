using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace Howbot.Core.Interfaces;

public interface IInteractionService
{
  Task Initialize();
  Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId);
  Task RegisterCommandsGloballyAsync();
  Task<IResult> ExecuteCommandAsync(SocketInteractionContext context);
  Task<IEnumerable<RestGlobalCommand>> GetGlobalApplicationCommandsAsync();
}
