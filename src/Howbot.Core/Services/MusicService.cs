using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Howbot.Core.Services;

public class MusicService : IMusicService
{
  private readonly ILoggerAdapter<MusicService> _logger;
  private readonly LavaNode _lavaNode;
  private readonly IServiceLocator _serviceLocator;

  public MusicService(ILoggerAdapter<MusicService> logger, LavaNode lavaNode, IServiceLocator serviceLocator)
  {
    _logger = logger;
    _lavaNode = lavaNode;
    _serviceLocator = serviceLocator;
  }

  #region Music Module Commands

  public async Task<CommandResponse> PlayBySearchTypeAsync(SearchType searchType, string searchRequest, IGuildUser user, IVoiceState voiceState,
    ITextChannel textChannel)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var voiceService = scope.ServiceProvider.GetRequiredService<IVoiceService>();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (user.Guild == null)
      {
        _logger.LogError("No guild exists for user in MusicService.PlayBySearchTypeAsync");
        return CommandResponse.CommandNotSuccessful("Unable to connect to voice channel");
      }

      // Join voice channel, if already connected return that
      var voiceServiceResponse = await voiceService.JoinVoiceAsync(user, textChannel);

      if (voiceServiceResponse.LavaPlayer == null)
      {
        throw new NullReferenceException(nameof(voiceServiceResponse.LavaPlayer));
      }

      _logger.LogDebug("Search Request: {SearchRequest} | Search Type: {SearchType}", searchRequest, searchType);
      var searchResponse = await lavaNode.SearchAsync(searchType, searchRequest);

      if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
      {
        _logger.LogInformation("No results were found for search query");
        return CommandResponse.CommandNotSuccessful("No results found for search");
      }

      await this.PlayTrack(voiceServiceResponse.LavaPlayer, searchResponse, user.Guild);
      return CommandResponse.CommandSuccessful(voiceServiceResponse.LavaPlayer);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.PlayBySearchTypeAsync");
      throw;
    }
  }

  public Task<CommandResponse> PlayByYouTubeSearch(string searchRequest, IGuildUser user, IVoiceState voiceState, ITextChannel textChannel)
  {
    throw new NotImplementedException();
  }

  #endregion

  private async Task PlayTrack(LavaPlayer<LavaTrack> lavaPlayer, SearchResponse searchResponse, IGuild guild)
  {
    this.AddToPlayerQueue(lavaPlayer, searchResponse, guild);
    
    // Check current player state
    if (lavaPlayer.PlayerState is PlayerState.Playing || (lavaPlayer.PlayerState is PlayerState.Paused &&
                                                          (lavaPlayer.Vueue.Count > 0 || lavaPlayer.Track != null)))
    {
      // Already playing or paused but has current track
      _logger.LogDebug("Already playing something.");
      return;
    }

    if (!lavaPlayer.Vueue.TryDequeue(out var lavaTrack))
    {
      _logger.LogDebug("Unable to dequeue lava player");
      return;
    }

    await lavaPlayer.PlayAsync(lavaTrack);
  }

  private void AddToPlayerQueue(LavaPlayer<LavaTrack> lavaPlayer, SearchResponse searchResponse, IGuild guild)
  {
    var originalQueueSize = lavaPlayer.Vueue.Count;

    if (IsSearchResponsePlaylist(searchResponse))
    {
      _logger.LogDebug("Adding {TrackCount} songs to queue", searchResponse.Tracks.Count);
      lavaPlayer.Vueue.Enqueue(searchResponse.Tracks);
    }
    else
    {
      var lavaTrack = searchResponse.Tracks.Count > 1
        ? searchResponse.Tracks.First()
        : throw new IndexOutOfRangeException(nameof(searchResponse.Tracks));
      
      lavaPlayer.Vueue.Enqueue(lavaTrack);
    }

    if (originalQueueSize == 0)
    {
      _logger.LogDebug("Added {Count} tracks to the queue", originalQueueSize);
    }
    else
    {
      var totalTracksAdded = lavaPlayer.Vueue.Count - originalQueueSize;
      _logger.LogDebug("Added {Count} tracks to the queue", originalQueueSize);
    }
  }

  private bool IsSearchResponsePlaylist(SearchResponse searchResponse)
  {
    return !string.IsNullOrEmpty(searchResponse.Playlist.Name);
  }
}
