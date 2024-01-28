using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Exceptions;
using Microsoft.Extensions.Logging;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class GeneralModule(
  InteractionService interactionService,
  IVoiceService voiceService,
  ILogger<GeneralModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
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
        var commandResponse = await voiceService.JoinVoiceChannelAsync(user, channel);
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
      logger.LogError(exception, nameof(JoinVoiceChannelCommandAsync));
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
          await voiceService.LeaveVoiceChannelAsync(user, channel).ConfigureAwait(false);

        await ModuleHelper.HandleCommandResponseAsync(commandResponse, Context).ConfigureAwait(false);
      }
      else
      {
        throw new CommandException("Unable to leave channel. Not in a voice channel or guild user null.");
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(LeaveVoiceChannelCommandAsync));
      throw;
    }
  }

  [SlashCommand(PingCommandName, PingCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.DM | ContextType.Guild)]
  [RequireBotPermission(GuildPermission.SendMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.SendMessages | GuildPermission.UseApplicationCommands |
                         GuildPermission.ViewChannel)]
  public async Task PingCommandAsync()
  {
    try
    {
      logger.LogDebug("Ping command invoked");

      await Context.Interaction.RespondAsync("Ping?").ConfigureAwait(false);

      var client = Context.Client;
      var responseTime = await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(false);
      var latency = client.Latency;
      var message = $"Pong! Response time: {responseTime.CreatedAt - Context.Interaction.CreatedAt}, " +
                    $"Latency: {latency}ms";

      await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Content = message)
        .ConfigureAwait(false);

      logger.LogDebug("Ping command completed");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PingCommandAsync));
      throw;
    }
  }

  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.DM | ContextType.Guild)]
  [RequireBotPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task HelpCommandAsync()
  {
    try
    {
      var commands = interactionService.SlashCommands;
      var commandList = string.Join("\n", commands.Select(c => $"`/{c.Name}`: {c.Description}"));

      var embedBuilder = new EmbedBuilder { Title = "Command List", Description = commandList };

      await RespondAsync(embeds: new[] { embedBuilder.Build() }).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(HelpCommandAsync));
      throw;
    }
  }
}
