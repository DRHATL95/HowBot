using Discord;
using Discord.Interactions;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Core.Modules;

public class AdminModule(ILoggerAdapter<AdminModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(Constants.Commands.PurgeCommandName, Constants.Commands.PurgeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageMessages |
                         GuildPermission.UseApplicationCommands)]
  [RequireOwner]
  public async Task PurgeCommandAsync()
  {
    await DeferAsync();

    try
    {
      // Due to limitation in Discord API, can only bulk delete messages up to 14 days old
      var bulkDeleteDate = DateTime.Now.AddDays(-14);
      // Get up to 10,000 messages
      var messages = (await Context.Channel.GetMessagesAsync(Constants.MaximumMessageCount).FlattenAsync()).ToList();

      if (messages.Any())
      {
        if (Context.Channel is not ITextChannel channel)
        {
          return;
        }

        var responseMessage =
          await ModifyOriginalResponseAsync(properties => properties.Content = "Purging messages..");

        if (responseMessage is null)
        {
          return;
        }

        var bulkMessagesToDelete =
          messages.Where(x => x.Id != responseMessage.Id && x.Timestamp.DateTime >= bulkDeleteDate).ToList();

        if (bulkMessagesToDelete.Any())
        {
          await channel.DeleteMessagesAsync(bulkMessagesToDelete);
        }

        // Run new task to delete messages > 14 days old
        await Task.Run(async () =>
        {
          foreach (var message in messages.Where(x =>
                     x.Id != responseMessage.Id && x.Timestamp.DateTime <= bulkDeleteDate))
          {
            await message.DeleteAsync();
          }
        });
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = "Successfully purged all messages.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]",
        nameof(AdminModule), nameof(PurgeCommandAsync));
      throw;
    }
  }

  [SlashCommand(Constants.Commands.BanCommandName, Constants.Commands.BanCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.BanMembers |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.BanMembers)]
  public async Task BanUserCommandAsync(string username, string? reason = null)
  {
    await DeferAsync();

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        await FollowupAsync("Unable to find user in server.");
        return;
      }

      await Context.Guild.AddBanAsync(user, reason: reason);

      await FollowupAsync("Successfully permanently banned user.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]",
        nameof(AdminModule), nameof(BanUserCommandAsync));
      await FollowupAsync("Failed to permanently ban user.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.UnbanCommandName, Constants.Commands.UnbanCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.BanMembers |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.BanMembers)]
  public async Task UnbanUserCommandAsync(string username)
  {
    await DeferAsync();

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        return;
      }

      await Context.Guild.RemoveBanAsync(user);

      await FollowupAsync("Successfully unbanned user.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(UnbanUserCommandAsync));
      await FollowupAsync("Failed to unban user.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.CleanCommandName, Constants.Commands.CleanCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ViewChannel)]
  public async Task CleanCommandAsync(int amount)
  {
    await DeferAsync();

    try
    {
      var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
      if (Context.Channel is not ITextChannel channel)
      {
        return;
      }

      // Delete all messages from bot in the channel
      messages = messages.Where(x => x.Author.Id == Context.Client.CurrentUser.Id).ToList();

      await channel.DeleteMessagesAsync(messages);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(CleanCommandAsync));
      throw;
    }
  }

  [SlashCommand(Constants.Commands.MuteCommandName, Constants.Commands.MuteCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.MuteMembers |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.MuteMembers)]
  public async Task MuteUserCommandAsync(string username)
  {
    await DeferAsync();

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        await FollowupAsync("Unable to find user in server.");
        return;
      }

      var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted");
      if (role is null)
      {
        await FollowupAsync("Unable to find Muted role in server.");
        return;
      }

      await user.AddRoleAsync(role);

      await FollowupAsync("Successfully muted user.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(MuteUserCommandAsync));
      await FollowupAsync("Failed to mute user.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.UnmuteCommandName, Constants.Commands.UnmuteCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.MuteMembers |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.MuteMembers)]
  public async Task UnmuteUserCommandAsync(string username)
  {
    await DeferAsync();

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        await FollowupAsync("Unable to find user in server.");
        return;
      }

      var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Muted");
      if (role is null)
      {
        await FollowupAsync("Unable to find Muted role in server.");
        return;
      }

      await user.RemoveRoleAsync(role);

      await FollowupAsync("Successfully unmuted user.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(UnmuteUserCommandAsync));
      await FollowupAsync("Failed to unmute user.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.KickCommandName, Constants.Commands.KickCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.KickMembers |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.KickMembers)]
  public async Task KickUserCommandAsync(string username, string? reason = null)
  {
    await DeferAsync();

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        await FollowupAsync("Unable to find user in server.");
        return;
      }

      await user.KickAsync(reason);

      await FollowupAsync("Successfully kicked user.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(KickUserCommandAsync));
      await FollowupAsync("Failed to kick user.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.SlowmodeCommandName, Constants.Commands.SlowmodeCommandDescription, true,
    RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageChannels |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.ManageChannels)]
  public async Task SlowmodeCommandAsync(int seconds)
  {
    await DeferAsync();

    try
    {
      if (Context.Channel is not ITextChannel channel)
      {
        return;
      }

      await channel.ModifyAsync(properties => properties.SlowModeInterval = seconds);

      await FollowupAsync($"Successfully set slowmode to {seconds} seconds.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SlowmodeCommandAsync));
      await FollowupAsync("Failed to set slowmode.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.LockCommandName, Constants.Commands.LockCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageChannels |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.ManageChannels)]
  public async Task LockCommandAsync()
  {
    await DeferAsync();

    try
    {
      if (Context.Channel is not ITextChannel channel)
      {
        return;
      }

      await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));

      await FollowupAsync("Successfully locked channel.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(LockCommandAsync));
      await FollowupAsync("Failed to lock channel.");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.UnlockCommandName, Constants.Commands.UnlockCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageChannels |
                         GuildPermission.UseApplicationCommands)]
  [RequireBotPermission(GuildPermission.Administrator | GuildPermission.ManageChannels)]
  public async Task UnlockCommandAsync()
  {
    await DeferAsync();

    try
    {
      if (Context.Channel is not ITextChannel channel)
      {
        return;
      }

      await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.InheritAll);

      await FollowupAsync("Successfully unlocked channel.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(UnlockCommandAsync));
      await FollowupAsync("Failed to unlock channel.");
      throw;
    }
  }

  [Group("settings", "Settings commands for the bot.")]
  public class SettingsGroup(
    ILoggerAdapter<SettingsGroup> logger,
    IMusicService musicService,
    IServiceProvider serviceProvider) : InteractionModuleBase<SocketInteractionContext>
  {
    // Create a command to display all setting information
    [SlashCommand("info", "Display all current settings for the server.", false, RunMode.Async)]
    public async Task InfoCommandAsync()
    {
      await DeferAsync();

      try
      {
        // Get scoped database service from provider
        using var scope = serviceProvider.CreateScope();
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

        // Get the guild settings from the database
        var guild = databaseService.GetGuildById(Context.Guild.Id);
        if (guild is null)
        {
          await FollowupAsync("Unable to find guild in database.");
          return;
        }

        // Send response to user
        await FollowupAsync(
          $"Prefix: `{guild.Prefix}`\nVolume: `{guild.Volume}%`\nMusic Search Provider: `{guild.SearchProvider}`");
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(InfoCommandAsync));
        throw;
      }
    }

    [SlashCommand("prefix", "Change the bot prefix for the server.", false, RunMode.Async)]
    public async Task PrefixCommandAsync(
      [Summary(Constants.Commands.SettingsPrefixArgumentName,
        Constants.Commands.SettingsPrefixArgumentDescription)]
      string newPrefix)
    {
      await DeferAsync();

      try
      {
        // Get scoped database service from provider
        using var scope = serviceProvider.CreateScope();
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

        // Update the prefix in the database
        await databaseService.UpdateGuildPrefixAsync(Context.Guild.Id, newPrefix);

        // Send response to user
        await FollowupAsync($"Successfully updated prefix to `{newPrefix}`.");
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(PrefixCommandAsync));
        throw;
      }
    }

    [SlashCommand("provider", "Update the music search provider for resolving search queries.", false, RunMode.Async)]
    public async Task ProviderCommandAsync(
      [Summary(Constants.Commands.SettingsProviderArgumentName, Constants.Commands.SettingsProviderArgumentDescription)]
      SearchProviderTypes provider)
    {
      await DeferAsync();

      try
      {
        // Get scoped database service from provider
        using var scope = serviceProvider.CreateScope();
        var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

        // Update the search provider in the database
        await databaseService.UpdateSearchProviderAsync(Context.Guild.Id, provider);

        // Send response to user
        await FollowupAsync($"Successfully updated search provider to `{provider}`.");
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(ProviderCommandAsync));
        throw;
      }
    }

    [SlashCommand(Constants.Commands.VolumeCommandName, Constants.Commands.VolumeCommandDescription, false,
      RunMode.Async)]
    public async Task VolumeCommandAsync(
      [Summary(Constants.Commands.SettingsVolumeArgumentName, Constants.Commands.SettingsVolumeArgumentDescription)]
      int? volume = null)
    {
      await DeferAsync();

      try
      {
        var player = await musicService.GetPlayerByGuildIdAsync(Context.Guild.Id);
        if (player is null)
        {
          await ModifyOriginalResponseAsync(properties =>
            properties.Content = "Unable to find player in server.");

          return;
        }

        if (!volume.HasValue)
        {
          // Respond with the current volume
          await ModifyOriginalResponseAsync(properties =>
            properties.Content = $"🔊 Current volume is {player.Volume * 100}%");

          return;
        }

        // Update the volume in the database
        var commandResponse = await musicService.ChangeVolumeAsync(player, volume.Value);

        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);

          if (string.IsNullOrEmpty(commandResponse.Message))
          {
            await DeleteOriginalResponseAsync();
            return;
          }
        }

        // Send response to user
        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(VolumeCommandAsync));
        throw;
      }
    }
  }
}
