using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
  [NotNull] private readonly InteractionService _interactionService;
  [NotNull] private readonly IVoiceService _voiceService;
  [NotNull] private readonly ILoggerAdapter<GeneralModule> _logger;

  public GeneralModule([NotNull] InteractionService interactionService,[NotNull] IVoiceService voiceService,[NotNull] ILoggerAdapter<GeneralModule> logger)
  {
    _interactionService = interactionService;
    _voiceService = voiceService;
    _logger = logger;
  }

  [SlashCommand(JoinCommandName, JoinCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoiceCommandPermission)]
  [RequireUserPermission(GuildUserVoiceCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task JoinCommandAsync()
  {
    try
    {
      await DeferAsync().ConfigureAwait(false);

      if (Context.User is IGuildUser user)
      {
        CommandResponse commandResponse = await _voiceService.JoinVoiceChannelAsync(user);
        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);
        }

        await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(LeaveCommandName, LeaveCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task LeaveVoiceChannelCommandAsync()
  {
    try
    {
      await DeferAsync().ConfigureAwait(false);

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        CommandResponse commandResponse = await _voiceService.LeaveVoiceChannelAsync(user, channel).ConfigureAwait(false);

        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);
        }
      }

      await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(LeaveVoiceChannelCommandAsync));
      throw;
    }
  }

  [SlashCommand(PingCommandName, PingCommandDescription, true, RunMode.Async)]
  public async Task PingCommandAsync()
  {
    try
    {
      var channel = Context.Channel;
      var client = Context.Client;

      var replyMessage = await channel.SendMessageAsync("Ping?").ConfigureAwait(false);

      var latency = client.Latency;
      var message =
        $"Pong! Bot WebSocket latency {latency}ms. Discord API latency {(DateTimeOffset.UtcNow - replyMessage.CreatedAt).TotalMilliseconds}ms";

      await Context.Channel.SendMessageAsync(message,
        messageReference: new MessageReference(replyMessage.Id, replyMessage.Channel.Id)).ConfigureAwait(false);

      await replyMessage.DeleteAsync().ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [NotNull]
  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  public Task HelpCommandAsync()
  {
    var commands = _interactionService.SlashCommands;
    var commandList = string.Join("\n", commands.Select(c => $"`/{c.Name}`: {c.Description}"));

    var embedBuilder = new EmbedBuilder { Title = "Command List", Description = commandList };

    RespondAsync(embeds: new[] { embedBuilder.Build() }).ConfigureAwait(false);

    return Task.CompletedTask;
  }
}
