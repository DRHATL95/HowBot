using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Exceptions;
using Microsoft.Extensions.DependencyInjection;
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
      await DeferAsync(true);

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        var commandResponse = await voiceService.JoinVoiceChannelAsync(user, channel);
        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);

          await FollowupAsync(commandResponse.Message);

          return;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          await FollowupAsync(commandResponse.Message);
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
      await DeferAsync();

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        var commandResponse =
          await voiceService.LeaveVoiceChannelAsync(user, channel);

        await ModuleHelper.HandleCommandResponseAsync(commandResponse, Context);
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
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.SendMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.SendMessages | GuildPermission.UseApplicationCommands |
                         GuildPermission.ViewChannel)]
  public async Task PingCommandAsync()
  {
    try
    {
      logger.LogDebug("Ping command invoked");

      await Context.Interaction.RespondAsync("Ping?");

      var client = Context.Client;
      var interactionMessage = await Context.Interaction.GetOriginalResponseAsync();
      var latency = client.Latency;
      var responseTime = interactionMessage.CreatedAt - Context.Interaction.CreatedAt;
      var message = $"Pong! Response time: {Math.Round(responseTime.TotalSeconds, 2)}s, " +
                    $"Latency: {latency}ms";

      await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Content = message);

      logger.LogDebug("Ping command completed");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PingCommandAsync));
      throw;
    }
  }

  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task HelpCommandAsync(
    [Summary("command", "The name of the command to get help for.")]
    string commandName = null)
  {
    try
    {
      var commands = interactionService.SlashCommands
        .Where(c => !c.Preconditions.OfType<RequireOwnerAttribute>().Any()); // Ignore owner-only commands
      if (!string.IsNullOrEmpty(commandName))
      {
        // If a command name is provided, find the command and return its description and example
        var command = commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command != null)
        {
          var example = ModuleHelper.CommandExampleDictionary.GetValueOrDefault(command.Name, "No example available");
          var embedBuilder = new EmbedBuilder
          {
            Title = $"{command.Name}",
            Description = $"{command.Description}\nExample: {example}",
            Color = Constants.ThemeColor
          };

          await RespondAsync(embeds: [embedBuilder.Build()]);
        }
        else
        {
          await RespondAsync($"No command found with the name `{commandName}`.");
        }
      }
      else
      {
        // If no command name is provided, return the list of all commands as before
        var groupedCommands = commands.GroupBy(c => c.Module.Name);

        var embedBuilder = new EmbedBuilder { Title = "Command List", Color = Constants.ThemeColor };

        foreach (var group in groupedCommands)
        {
          var moduleName = group.Key.Replace("Module", ""); // Remove the word "Module" from the group key
          var commandList = new List<string>();
          var continuationFields = new List<EmbedFieldBuilder>();

          foreach (var command in group)
          {
            var commandInfo = $"`/{command.Name}`: {command.Description}\n"; // Removed example

            // If adding the next command would exceed the limit and there are already commands in the list, add the current list as a field and start a new list
            if (commandList.Any() && commandList.Sum(c => c.Length) + commandInfo.Length > 1024)
            {
              continuationFields.Add(new EmbedFieldBuilder
              {
                Name = $"{moduleName} (cont.)", Value = string.Join("\n", commandList)
              });
              commandList.Clear();
            }

            commandList.Add(commandInfo);
          }

          // Add main section as a field
          if (commandList.Any())
          {
            embedBuilder.AddField(moduleName, string.Join("\n", commandList));
          }

          // Add continuation sections as fields
          foreach (var field in continuationFields)
          {
            embedBuilder.AddField(field);
          }
        }

        await RespondAsync(embeds: [embedBuilder.Build()]);
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(HelpCommandAsync));
      throw;
    }
  }

  [Group("settings", "Settings commands")]
  public class SettingsGroup(/*IServiceProvider serviceProvider,*/ ILoggerAdapter<SettingsGroup> logger)
    : InteractionModuleBase<SocketInteractionContext>
  {
    [SlashCommand("provider", "Sets the bot default music search provider")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageGuild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireOwner] // Until table is updated to support new column
    public async Task SetProviderCommandAsync(
      [Summary("provider", "The new music search provider to use for queries.")]
      SearchProviderTypes provider)
    {
      try
      {
        await RespondAsync("This command is not yet implemented.");

        // await DeferAsync();

        /*using var scope = serviceProvider.CreateScope();
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

        var guild = databaseService.GetGuildById(Context.Guild.Id);
        if (guild is null)
        {
          throw new CommandException("Guild not found in database.");
        }

        await databaseService.UpdateSearchProviderAsync(guild.Id, provider);*/
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(SetProviderCommandAsync));
        throw;
      }
    }
  }
}
