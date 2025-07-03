using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static Howbot.Application.Models.Discord.Messages.Responses;

namespace Howbot.Application.Attributes;

public class RequireGuildUserInVoiceChannelAttribute : PreconditionAttribute
{
  public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo,
    IServiceProvider services)
  {
    return context.User is not SocketGuildUser user
      ? Task.FromResult(PreconditionResult.FromError("User is not a guild user."))
      : Task.FromResult(user.VoiceChannel is null
        ? PreconditionResult.FromError(BotUserVoiceConnectionRequired)
        : PreconditionResult.FromSuccess());
  }
}
