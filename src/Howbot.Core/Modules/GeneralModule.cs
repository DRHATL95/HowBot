using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Exceptions;
using Microsoft.Extensions.Logging;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class GeneralModule(
  IHttpService httpService,
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
      await DeferAsync();

      if (Context.User is IGuildUser user && Context.Channel is IGuildChannel channel)
      {
        var commandResponse =
          await voiceService.JoinVoiceChannelAsync(user, channel);

        await ModuleHelper.HandleCommandResponseAsync(commandResponse, Context);
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
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task PingCommandAsync()
  {
    try
    {
      await Context.Interaction.RespondAsync("Ping?");

      var interactionMessage = await Context.Interaction.GetOriginalResponseAsync();
      var responseTime = Math.Round((Context.Interaction.CreatedAt - interactionMessage.CreatedAt).TotalSeconds, 2);

      var message = $"Pong! API Latency: {responseTime}s. " +
                    $"Bot Latency: {Context.Client.Latency}ms";

      await Context.Interaction.ModifyOriginalResponseAsync(properties => properties.Content = message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PingCommandAsync));
      throw;
    }
  }

  [SlashCommand(SayCommandName, SayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.SendMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task SayCommandAsync(string message)
  {
    try
    {
      await RespondAsync(message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SayCommandAsync));
      throw;
    }
  }

  [SlashCommand(HelpCommandName, HelpCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task HelpCommandAsync(
    [Summary(HelpCommandArgumentName, HelpCommandArgumentDescription)]
    string? commandName = null)
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
          var examples =
            ModuleHelper.CommandExampleDictionary.GetValueOrDefault(command.Name,
              new List<string> { "No example available" });
          var embedBuilder = new EmbedBuilder
          {
            Title = $"{command.Name}",
            Description = $"{command.Description}\nExamples:\n{string.Join("\n", examples)}",
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
          if (moduleName == "SettingsGroup") // Special case for SettingsGroupModule
          {
            moduleName = "Settings";
          }

          if (moduleName == "EscapeFromTarkovGroup")
          {
            moduleName = "Escape From Tarkov";
          }

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

  [SlashCommand(CatCommandName, CatCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.SendMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task CatCommandAsync([Summary("limit", "The limit of images to return. Max 10.")] int limit = 1)
  {
    if (limit is < 1 or > 10)
    {
      await RespondAsync("The limit must be between 1 and 10.");
      return;
    }

    try
    {
      await DeferAsync();

      // Either returns a single cat image or a list of cat images separated by commas
      var catImageUrl = await httpService.GetRandomCatImageUrlAsync(limit);

      if (limit == 1)
      {
        var embedBuilder = new EmbedBuilder
        {
          Title = "Random Cat Image", ImageUrl = catImageUrl, Color = Constants.ThemeColor
        };

        await ModifyOriginalResponseAsync(properties => properties.Embed = embedBuilder.Build());
      }
      else
      {
        var embedBuilder = new EmbedBuilder { Title = "Random Cat Images", Color = Constants.ThemeColor };

        // Convert the comma-separated string to a list of URLs
        var urls = catImageUrl.Split(",").ToList();
        for (var i = 0; i < urls.Count; i++)
        {
          var count = i + 1;
          embedBuilder.AddField($"Cat Image #{count}", urls[i]);
        }

        await ModifyOriginalResponseAsync(properties => properties.Embed = embedBuilder.Build());
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(CatCommandAsync));
      throw;
    }
  }

  [SlashCommand(DogCommandName, DogCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.SendMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.UseApplicationCommands | GuildPermission.SendMessages |
                         GuildPermission.ViewChannel)]
  public async Task DogCommandAsync([Summary("limit", "The limit of images to return. Max 10.")] int limit = 1)
  {
    if (limit is < 1 or > 10)
    {
      await RespondAsync("The limit must be between 1 and 10.");
      return;
    }

    try
    {
      await DeferAsync();

      // Either returns a single dog image or a list of dog images separated by commas
      var dogImageUrl = await httpService.GetRandomDogImageUrlAsync(limit);

      if (limit == 1)
      {
        var embedBuilder = new EmbedBuilder
        {
          Title = "Random Dog Image", ImageUrl = dogImageUrl, Color = Constants.ThemeColor
        };

        await ModifyOriginalResponseAsync(properties => properties.Embed = embedBuilder.Build());
      }
      else
      {
        var embedBuilder = new EmbedBuilder { Title = "Random Dog Images", Color = Constants.ThemeColor };

        // Convert the comma-separated string to a list of URLs
        var urls = dogImageUrl.Split(",").ToList();
        for (var i = 0; i < urls.Count; i++)
        {
          var count = i + 1;
          embedBuilder.AddField($"Dog Image #{count}", urls[i]);
        }

        await ModifyOriginalResponseAsync(properties => properties.Embed = embedBuilder.Build());
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(DogCommandAsync));
      throw;
    }
  }
}
