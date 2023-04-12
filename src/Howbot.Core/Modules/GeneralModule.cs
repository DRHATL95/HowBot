using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly InteractionService _interactionService;
  private readonly IVoiceService _voiceService;
  private readonly ILoggerAdapter<GeneralModule> _logger;

  public GeneralModule(InteractionService interactionService, IVoiceService voiceService, ILoggerAdapter<GeneralModule> logger)
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
      await DeferAsync();

      CommandResponse commandResponse = await _voiceService.JoinVoiceAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);
      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);
      }

      await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
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
      await DeferAsync();

      var commandResponse = await _voiceService.LeaveVoiceChannelAsync(Context.User as IGuildUser);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);
      }

      await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
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
      var replyMessage = await Context.Channel.SendMessageAsync("Ping?");

      var latency = Context.Client.Latency;
      var message =
        $"Pong! Bot WebSocket latency {latency}ms. Discord API latency {(DateTimeOffset.UtcNow - replyMessage.CreatedAt).TotalMilliseconds}ms";

      var editedMessage = await Context.Channel.SendMessageAsync(message,
        messageReference: new MessageReference(replyMessage.Id, replyMessage.Channel.Id));

      await replyMessage.DeleteAsync();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  public async Task HelpCommandAsync()
  {
    var commands = _interactionService.SlashCommands;
    var commandList = string.Join("\n", commands.Select(c => $"`/{c.Name}`: {c.Description}"));

    var embedBuilder = new EmbedBuilder { Title = "Command List", Description = commandList };

    await RespondAsync(embeds: new[] { embedBuilder.Build() });
  }
}
