using Discord;
using Discord.Interactions;
using Howbot.Core.Models;

namespace Howbot.Core.Attributes;

public class RequireGuildTextChat : PreconditionAttribute
{
  public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo,
    IServiceProvider services)
  {
    return Task.FromResult(context.Channel is ITextChannel
      ? PreconditionResult.FromSuccess()
      : PreconditionResult.FromError(Messages.Errors.InteractionTextChannelRequired));
  }
}
