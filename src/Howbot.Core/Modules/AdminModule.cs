using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;

namespace Howbot.Core.Modules;

public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly ILoggerAdapter<AdminModule> _logger;

  public AdminModule(ILoggerAdapter<AdminModule> logger)
  {
    _logger = logger;
  }

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
          await channel.DeleteMessagesAsync(bulkMessagesToDelete);
        }

        // Run new task to delete messages > 14 days old
        await Task.Run(async () =>
        {
          foreach (var message in messages.Where(x => x.Timestamp.DateTime <= bulkDeleteDate))
          {
            await message.DeleteAsync().ConfigureAwait(false);
          }
        }).ConfigureAwait(false);
      }

      await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]",
        nameof(AdminModule), nameof(PurgeCommandAsync));
      throw;
    }
  }
}
