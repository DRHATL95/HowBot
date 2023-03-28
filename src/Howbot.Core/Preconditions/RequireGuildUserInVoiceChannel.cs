using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static Howbot.Core.Messages.Responses;

namespace Howbot.Core.Preconditions;

public class RequireGuildUserInVoiceChannelAttribute : PreconditionAttribute
{
  public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo,
    IServiceProvider services)
  {
    return Task.FromResult(context.User is SocketGuildUser { VoiceChannel: { } }
      ? PreconditionResult.FromSuccess()
      : PreconditionResult.FromError(UserVoiceConnectionRequired));
  }
}
