using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Modules;

public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly ILoggerAdapter<AdminModule> _logger;

  public AdminModule(ILoggerAdapter<AdminModule> logger)
  {
    _logger = logger;
  }

  [SlashCommand("purge", "Purge current text channel from ALL messages.", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireOwner]
  public async Task PurgeCommandAsync()
  {
    await DeferAsync().ConfigureAwait(false);

    try
    {
      var messages = await Context.Channel.GetMessagesAsync(int.MaxValue).FlattenAsync().ConfigureAwait(false);

      if (messages.Any())
      {
        if (Context.Channel is not ITextChannel channel) return;

        var responseMessage = await ModifyOriginalResponseAsync(properties => properties.Content = $"Purging messages..").ConfigureAwait(false);

        if (responseMessage is null) return;

        var bulkMessages = messages.Where(x => x.Id != responseMessage.Id && x.Timestamp.DateTime >= DateTime.Now.AddDays(-14));

        if (bulkMessages.Any())
        {
          await channel.DeleteMessagesAsync(bulkMessages);
        }

        // Run new task to delete messages > 14 days old
        await Task.Run(async () =>
        {
          foreach (var message in messages.Where(x => x.Timestamp.DateTime <= DateTime.Now.AddDays(-14)))
          {
            await message.DeleteAsync().ConfigureAwait(false);
          }

        }).ConfigureAwait(false);
      }

      await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in Module [{ModuleName}] Command [{CommandName}]", nameof(AdminModule), nameof(PurgeCommandAsync));
      throw;
    }
  }

}
