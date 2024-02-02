using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;

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
        if (Context.Channel is not ITextChannel channel) return;

        var responseMessage =
          await ModifyOriginalResponseAsync(properties => properties.Content = $"Purging messages..");

        if (responseMessage is null) return;

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
  public async Task BanUserCommandAsync(string username, string reason = null)
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
  public async Task UnbanUserCommandAsync(string username, string reason = null)
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
}
