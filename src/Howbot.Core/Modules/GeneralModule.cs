using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly InteractionService _interactionService;
  private readonly ILoggerAdapter<GeneralModule> _logger;

  public GeneralModule(InteractionService interactionService, ILoggerAdapter<GeneralModule> logger)
  {
    _interactionService = interactionService;
    _logger = logger;
  }

  [SlashCommand(Constants.Commands.PingCommandName, Constants.Commands.PingCommandDescription, true, RunMode.Async)]
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

  [SlashCommand("help", "See all available commands for the bot", true, RunMode.Async)]
  public async Task HelpCommandAsync()
  {
    var commands = _interactionService.SlashCommands;
    var commandList = string.Join("\n", commands.Select(c => $"`/{c.Name}`: {c.Description}"));

    var embedBuilder = new EmbedBuilder { Title = "Command List", Description = commandList };

    await RespondAsync(embeds: new[] { embedBuilder.Build() });
  }
}
