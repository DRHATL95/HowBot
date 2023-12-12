using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
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
  [RequireGuildTextChat] // TODO: May not be needed because of RequireContext
  public async Task PurgeCommandAsync()
  {
    await DeferAsync().ConfigureAwait(false);

    try
    {
      // Due to limitation in Discord API, can only bulk delete messages up to 14 days old
      var bulkDeleteDate = DateTime.Now.AddDays(-14);
      // Get up to 10,000 messages
      var messages = (await Context.Channel.GetMessagesAsync(Constants.MaximumMessageCount).FlattenAsync()
        .ConfigureAwait(false)).ToList();

      if (messages.Any())
      {
        if (Context.Channel is not ITextChannel channel) return;

        var responseMessage =
          await ModifyOriginalResponseAsync(properties => properties.Content = $"Purging messages..")
            .ConfigureAwait(false);

        if (responseMessage is null) return;

        var bulkMessagesToDelete =
          messages.Where(x => x.Id != responseMessage.Id && x.Timestamp.DateTime >= bulkDeleteDate).ToList();

        if (bulkMessagesToDelete.Any())
        {
          await channel.DeleteMessagesAsync(bulkMessagesToDelete).ConfigureAwait(false);
        }

        // Run new task to delete messages > 14 days old
        await Task.Run(async () =>
        {
          foreach (var message in messages.Where(x =>
                     x.Id != responseMessage.Id && x.Timestamp.DateTime <= bulkDeleteDate))
          {
            await message.DeleteAsync().ConfigureAwait(false);
          }
        }).ConfigureAwait(false);
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = "Successfully purged all messages.")
        .ConfigureAwait(false);
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
    await DeferAsync().ConfigureAwait(false);

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        await FollowupAsync("Unable to find user in server.").ConfigureAwait(false);
        return;
      }

      await Context.Guild.AddBanAsync(user, reason: reason).ConfigureAwait(false);

      await FollowupAsync("Successfully permanently banned user.").ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]",
        nameof(AdminModule), nameof(BanUserCommandAsync));
      await FollowupAsync("Failed to permanently ban user.").ConfigureAwait(false);
      throw;
    }
  }

  [SlashCommand("unban", "Unban a user from the server.", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  public async Task UnBanUserCommandAsync(string username, string reason = null)
  {
    await DeferAsync().ConfigureAwait(false);

    try
    {
      var user = Context.Guild.Users.FirstOrDefault(x => x.Username == username);
      if (user is null)
      {
        return;
      }

      await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);

      await FollowupAsync("Successfully unbanned user.").ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]",
        nameof(AdminModule), nameof(UnBanUserCommandAsync));
      await FollowupAsync("Failed to unban user.").ConfigureAwait(false);
      throw;
    }
  }
}
