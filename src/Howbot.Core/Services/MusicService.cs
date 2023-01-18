using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Node;
using Victoria.Player;
using Victoria.Player.Filters;
using Victoria.Responses.Search;

namespace Howbot.Core.Services;

public class MusicService : IMusicService
{
  private readonly ILoggerAdapter<MusicService> _logger;
  private readonly IServiceLocator _serviceLocator;

  public MusicService(ILoggerAdapter<MusicService> logger, IServiceLocator serviceLocator)
  {
    _logger = logger;
    _serviceLocator = serviceLocator;
  }

  #region Music Module Commands

  public async Task<CommandResponse> PlayBySearchTypeAsync(SearchType searchType, string searchRequest, IGuildUser user, IVoiceState voiceState, ITextChannel textChannel)
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
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> PauseTrackAsync(IGuild guild)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug("Unable to pause, player is not defined");
        return CommandResponse.CommandNotSuccessful("I am not connected to a voice channel");
      }

      if (lavaPlayer.PlayerState is not PlayerState.Playing)
      {
        _logger.LogDebug("Unable to pause, player is not playing");
        return CommandResponse.CommandNotSuccessful("Unable to pause anything. There is nothing playing!");
      }

      _logger.LogDebug("Pausing current track");
      await lavaPlayer.PauseAsync();

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.PauseCurrentPlayingTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ResumeTrackAsync(IGuild guild)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug("Unable to resume track, player is not defined.");
        return CommandResponse.CommandNotSuccessful("I am not connected to a voice channel");
      }

      if (lavaPlayer.PlayerState is not PlayerState.Paused or PlayerState.Stopped)
      {
        _logger.LogDebug("Cannot resume a track that isn't paused or stopped.");
        return CommandResponse.CommandNotSuccessful("Unable to resume a track that's not stopped or paused.");
      }
      
      _logger.LogDebug("Resuming track.");
      await lavaPlayer.ResumeAsync();

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.PauseCurrentPlayingTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SkipTrackAsync(IGuild guild, int numberOfTracks)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(Messages.Debug.ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }

      if (numberOfTracks > 0)
      {
        if (numberOfTracks > lavaPlayer.Vueue.Count)
        {
          _logger.LogDebug(Messages.Debug.ClientQueueOutOfBounds);
          return CommandResponse.CommandNotSuccessful(Messages.Responses.BotSkipQueueOutOfBounds);
        }
        
        // Skipped ahead multiple places in queue
        _logger.LogDebug("Song before skip: [{SongName} - {Artist}]", lavaPlayer.Track.Title, lavaPlayer.Track.Author);
        _logger.LogDebug("Skipping {Count} tracks in queue", numberOfTracks);
        lavaPlayer.Vueue.RemoveRange(0, (lavaPlayer.Vueue.Count - 1));
        _logger.LogDebug("Song after skip: [{SongName} - {Artist}]", lavaPlayer.Track.Title, lavaPlayer.Track.Author);

        _logger.LogDebug(Messages.Debug.NowPlaying);
        await lavaPlayer.PlayAsync(lavaPlayer.Track);
        return CommandResponse.CommandSuccessful();
      }

      _logger.LogDebug(Messages.Debug.SkipNextTrack);
      await lavaPlayer.SkipAsync();
      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.SkipTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SeekTrackAsync(IGuild guild, TimeSpan seekPosition)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(Messages.Debug.ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }

      if (seekPosition.TotalSeconds <= 0)
      {
        _logger.LogDebug("Not given proper seek position");
        return CommandResponse.CommandNotSuccessful("Please give a valid seek time");
      }

      if (!lavaPlayer.Track.CanSeek)
      {
        _logger.LogDebug("Current track is unable to seek");
        return CommandResponse.CommandNotSuccessful("This track is unable to seek");
      }

      var maxSeekSecondsAllowed = lavaPlayer.Track.Duration.TotalSeconds;
      if (maxSeekSecondsAllowed < seekPosition.TotalSeconds || lavaPlayer.Track.Duration < seekPosition)
      {
        _logger.LogDebug("Unable to seek to that position for this track");
        return CommandResponse.CommandNotSuccessful("Unable to seek to that position");
      }

      await lavaPlayer.SeekAsync(seekPosition);

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ChangeVolumeAsync(IGuild guild, int? newVolume)
  {
    try
    {
      // Since second param is optional, if 0 do nothing but return command success
      if (!newVolume.HasValue) return CommandResponse.CommandSuccessful();
      
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(Messages.Debug.ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }

      if (newVolume is < 0 or > 100)
      {
        _logger.LogError("Volume is incorrect value");
        return CommandResponse.CommandNotSuccessful("Incorrect volume value. Must be between 0-100.");
      }

      if (newVolume == lavaPlayer.Volume)
      {
        // Do nothing
        _logger.LogDebug("Volumes are the same, doing nothing");
        return CommandResponse.CommandNotSuccessful();
      }
      
      _logger.LogDebug("Setting player volume to {Volume}", newVolume.Value);
      await lavaPlayer.SetVolumeAsync(newVolume.Value);
      
      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> NowPlayingAsync(IGuildUser user, ITextChannel textChannel)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();
      var embedService = scope.ServiceProvider.GetRequiredService<IEmbedService>();
      var guild = user.Guild;

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(Messages.Debug.ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }

      if (lavaPlayer.Track == null || lavaPlayer.PlayerState is not PlayerState.Playing)
      {
        _logger.LogDebug("Not playing anything");
        return CommandResponse.CommandNotSuccessful("Not playing anything");
      }

      var embed = await embedService.GenerateMusicNowPlayingEmbedAsync(lavaPlayer.Track, user, textChannel);
      if (embed == null)
      {
        _logger.LogError("Unable to generate embed");
        return CommandResponse.CommandNotSuccessful("Unable to generate embed");
      }

      return CommandResponse.CommandSuccessful(embed);
    }
    catch (Exception exception)
    {
      Console.WriteLine(exception);
      throw;
    }
  }

  public async Task<CommandResponse> ApplyAudioFilterAsync<T>(IGuild guild, T filter)
  {
    try
    {
      using var scope = _serviceLocator.CreateScope();
      var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

      if (!lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(Messages.Debug.ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }

      if (lavaPlayer.Track == null || lavaPlayer.PlayerState is PlayerState.None)
      {
        _logger.LogDebug("Cannot apply filter to player. Nothing is playing");
        return CommandResponse.CommandNotSuccessful("Cannot apply filter. Nothing is playing");
      }

      if (filter is IFilter audioFilter)
      {
        await lavaPlayer.ApplyFilterAsync(audioFilter);
        return CommandResponse.CommandSuccessful();
      }
      else
      {
        _logger.LogError("Unable to apply filter. Incorrect filter provided.");
        return CommandResponse.CommandNotSuccessful("Unable to apply filter. Wrong filter provided");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
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
      _logger.LogDebug("Added 1 track to the queue", originalQueueSize);
    }
    else
    {
      var totalTracksAdded = lavaPlayer.Vueue.Count - originalQueueSize;
      _logger.LogDebug("Added {Count} tracks to the queue", totalTracksAdded);
    }
  }

  private static bool IsSearchResponsePlaylist(SearchResponse searchResponse)
  {
    return !string.IsNullOrEmpty(searchResponse.Playlist.Name);
  }
}
