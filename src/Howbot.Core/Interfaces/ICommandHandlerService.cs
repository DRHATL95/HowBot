using Discord.Interactions;
using Howbot.Core.Models.Commands;

namespace Howbot.Core.Interfaces;

public interface ICommandHandlerService
{
  Task<CommandResponse> HandleCommandRequestAsync(SocketInteractionContext socketInteractionContext,
    CancellationToken cancellationToken = default);

  Task<ApiCommandResponse> HandleCommandRequestAsync(string commandJson,
    CancellationToken cancellationToken = default);
}
