using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class VideoModule(IHttpService httpService, ILoggerAdapter<VideoModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("w2g", "Creates a Watch2gether room")]
  [RequireContext(ContextType.Guild)]
  [SuppressMessage("ReSharper", "UnusedMember.Global")]
  public async Task WatchTogetherCommandAsync([Summary("url", "The url to create the room with.")] string url = null)
  {
    await DeferAsync();

    try
    {
      var responseMessage =
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Creating Watch2gether room..");

      if (responseMessage is null) return;

      var watch2GetherRoomUrl = await httpService.CreateWatchTogetherRoomAsync(url);
      if (string.IsNullOrEmpty(watch2GetherRoomUrl))
      {
        throw new Exception();
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = $"Watch2gether room created: {watch2GetherRoomUrl}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating Watch2gether room");
      
      await ModifyOriginalResponseAsync(properties => properties.Content = $"Error creating Watch2gether room");
    }
  }
}
