using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;

namespace Howbot.Core.Modules;

public class VideoModule(IHttpService httpService, ILoggerAdapter<VideoModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(Constants.Commands.WatchTogetherCommandName, Constants.Commands.WatchTogetherCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task WatchTogetherCommandAsync([Summary("url", "The url to create the room with.")] string url)
  {
    try
    {
      await RespondAsync("Creating Watch2gether room..");
      
      var watch2GetherRoomUrl = await httpService.CreateWatchTogetherRoomAsync(url);
      if (string.IsNullOrEmpty(watch2GetherRoomUrl))
      {
        throw new Exception();
      }

      await ModifyOriginalResponseAsync(properties =>
        properties.Content = $"Watch2gether room created: {watch2GetherRoomUrl}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating Watch2gether room");

      await ModifyOriginalResponseAsync(properties => properties.Content = "Error creating Watch2gether room");
    }
  }
}
