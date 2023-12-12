﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Core.Interfaces;
using Newtonsoft.Json;

namespace Howbot.Core.Modules;

public class VideoModule(IHttpService httpService, ILoggerAdapter<VideoModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("w2g", "Creates a Watch2gether room")]
  [RequireContext(ContextType.Guild)]
  public async Task WatchTogetherCommandAsync([Summary("url", "The url to create the room with.")]string url = null)
  {
    await DeferAsync().ConfigureAwait(false);

    try
    {
      var responseMessage =
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Creating Watch2gether room..")
          .ConfigureAwait(false);

      if (responseMessage is null) return;

      var watch2GetherRoomUrl = await httpService.CreateWatchTogetherRoomAsync(url).ConfigureAwait(false);
      if (string.IsNullOrEmpty(watch2GetherRoomUrl))
      {
        throw new Exception();
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = $"Watch2gether room created: {watch2GetherRoomUrl}")
        .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating Watch2gether room");
      await ModifyOriginalResponseAsync(properties => properties.Content = $"Error creating Watch2gether room")
        .ConfigureAwait(false);
    }
  }
} 
