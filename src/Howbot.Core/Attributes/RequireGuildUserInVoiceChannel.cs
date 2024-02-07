using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static Howbot.Core.Models.Messages.Responses;

namespace Howbot.Core.Attributes;

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
