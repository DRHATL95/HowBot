using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly InteractionService _interactionService;
  private readonly ILogger<GeneralModule> _logger;
  private readonly IVoiceService _voiceService;

  public GeneralModule(InteractionService interactionService, IVoiceService voiceService,
    ILogger<GeneralModule> logger)
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
  public async Task JoinVoiceChannelCommandAsync()
  {
    try
    {
      await DeferAsync(true).ConfigureAwait(false);

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        var commandResponse = await _voiceService.JoinVoiceChannelAsync(user, channel);
        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);

          await FollowupAsync(commandResponse.Message).ConfigureAwait(false);

          return;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          await FollowupAsync(commandResponse.Message).ConfigureAwait(false);
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(JoinVoiceChannelCommandAsync));
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
      await DeferAsync(true).ConfigureAwait(false);

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        CommandResponse commandResponse =
          await _voiceService.LeaveVoiceChannelAsync(user, channel).ConfigureAwait(false);

        await ModuleHelper.HandleCommandResponseAsync(commandResponse, Context).ConfigureAwait(false);
      }
      else
      {
        throw new CommandException("Unable to leave channel. Not in a voice channel or guild user null.");
      }
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
      _logger.LogDebug("Ping command invoked.");

      await Context.Interaction.RespondAsync("Ping?").ConfigureAwait(false);

      var client = Context.Client;
      var responseTime = await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);
      var latency = client.Latency;
      var message =
        $"Pong! Bot WebSocket latency {latency}ms. Discord API latency {(DateTimeOffset.UtcNow - responseTime.CreatedAt).TotalMilliseconds}ms";

      await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Content = message)
        .ConfigureAwait(false);

      _logger.LogDebug("Ping command completed.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PingCommandAsync));
      throw;
    }
  }

  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  public async Task HelpCommandAsync()
  {
    try
    {
      _logger.LogDebug("Help command invoked.");

      var commands = _interactionService.SlashCommands;
      var commandList = string.Join("\n", commands.Select(c => $"`/{c.Name}`: {c.Description}"));

      var embedBuilder = new EmbedBuilder { Title = "Command List", Description = commandList };

      await RespondAsync(embeds: new[] { embedBuilder.Build() }).ConfigureAwait(false);

      _logger.LogDebug("Help command completed.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(HelpCommandAsync));
      throw;
    }
  }
}
