using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Google.Apis.YouTube.v3;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Player.Filters;
using Victoria.Responses.Search;
using static Howbot.Core.Models.Messages.Debug;
using static Howbot.Core.Models.Messages.Responses;
using static Victoria.Player.PlayerState;

namespace Howbot.Core.Services;

public class MusicService : ServiceBase<MusicService>, IMusicService
{
  private readonly IEmbedService _embedService;
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<MusicService> _logger;
  private readonly IVoiceService _voiceService;
  private readonly YouTubeService _youTubeService;

  public MusicService(IVoiceService voiceService, IEmbedService embedService,
    LavaNode<Player<LavaTrack>, LavaTrack> lavaNode, YouTubeService youTubeService,
    ILoggerAdapter<MusicService> logger) : base(logger)
  {
    _voiceService = voiceService;
    _embedService = embedService;
    _lavaNode = lavaNode;
    _youTubeService = youTubeService;
    _logger = logger;
  }

  public async Task<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1)
  {
    var searchListRequest = _youTubeService.Search.List("snippet");

    searchListRequest.Type = "video";
    // For some reason if I want 1 result, I have to set the max results to 2?
    // TODO: Investigate
    searchListRequest.MaxResults = count == 1 ? 2 : count;
    searchListRequest.RelatedToVideoId = videoId;

    var response = await searchListRequest.ExecuteAsync();

    if (count <= 1)
    {
      return new[] { response.Items[0].Id.VideoId };
    }

    return response.Items.Select(item => item.Id.VideoId).ToList();
  }

  #region Music Module Commands

  public async Task<CommandResponse> PlayBySearchTypeAsync(SearchType searchType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel)
  {
    try
    {
      if (user.Guild == null)
      {
        _logger.LogError("No guild exists for user in MusicService.PlayBySearchTypeAsync");
        return CommandResponse.CommandNotSuccessful("Unable to connect to voice channel");
      }

      // Join voice channel, if already connected return that
      var voiceServiceResponse = await _voiceService.JoinVoiceAsync(user, textChannel);

      if (voiceServiceResponse.LavaPlayer == null)
      {
        throw new NullReferenceException(nameof(voiceServiceResponse.LavaPlayer));
      }

      _logger.LogDebug("Search Request: {SearchRequest} | Search Type: {SearchType}", searchRequest, searchType);
      var searchResponse = await _lavaNode.SearchAsync(searchType, searchRequest);

      if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
      {
        _logger.LogInformation("No results were found for search query");
        return CommandResponse.CommandNotSuccessful("No results found for search");
      }

      await PlayTrack(voiceServiceResponse.LavaPlayer, searchResponse, user);
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
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug("Unable to pause, player is not defined");
        return CommandResponse.CommandNotSuccessful("I am not connected to a voice channel");
      }

      if (lavaPlayer.PlayerState is not Playing)
      {
        _logger.LogDebug("Unable to pause, player is not playing");
        return CommandResponse.CommandNotSuccessful("Unable to pause anything. There is nothing playing!");
      }

      _logger.LogDebug("Pausing current track");
      await lavaPlayer.PauseAsync();

      return CommandResponse.CommandSuccessful(BotTrackPaused);
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
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug("Unable to resume track, player is not defined.");
        return CommandResponse.CommandNotSuccessful("I am not connected to a voice channel");
      }

      if (lavaPlayer.PlayerState is not Paused or Stopped)
      {
        _logger.LogDebug("Cannot resume a track that isn't paused or stopped.");
        return CommandResponse.CommandNotSuccessful("Unable to resume a track that's not stopped or paused.");
      }

      _logger.LogDebug(Resume);
      await lavaPlayer.ResumeAsync();

      return CommandResponse.CommandSuccessful(BotTrackResumed);
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
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      if (numberOfTracks > 0)
      {
        if (numberOfTracks > lavaPlayer.Vueue.Count)
        {
          _logger.LogDebug(ClientQueueOutOfBounds);
          return CommandResponse.CommandNotSuccessful(BotSkipQueueOutOfBounds);
        }

        // Skipped ahead multiple places in queue
        _logger.LogDebug("Song before skip: [{SongName} - {Artist}]", lavaPlayer.Track.Title, lavaPlayer.Track.Author);
        _logger.LogDebug("Skipping {Count} tracks in queue", numberOfTracks);
        lavaPlayer.Vueue.RemoveRange(0, lavaPlayer.Vueue.Count - 1);
        _logger.LogDebug("Song after skip: [{SongName} - {Artist}]", lavaPlayer.Track.Title, lavaPlayer.Track.Author);

        _logger.LogDebug(NowPlaying);
        await lavaPlayer.PlayAsync(lavaPlayer.Track);
        return CommandResponse.CommandSuccessful(BotTrackSkipped);
      }

      _logger.LogDebug(SkipNextTrack);
      await lavaPlayer.SkipAsync();
      return CommandResponse.CommandSuccessful(BotTrackSkipped);
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
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
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

      return CommandResponse.CommandSuccessful($"Seeking to {seekPosition:g}.");
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
      if (!newVolume.HasValue)
      {
        return CommandResponse.CommandSuccessful();
      }

      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
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

      return CommandResponse.CommandSuccessful($"Changing volume to {newVolume.Value}.");
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
      if (!_lavaNode.TryGetPlayer(user.Guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      if (lavaPlayer.Track == null || lavaPlayer.PlayerState is not Playing)
      {
        _logger.LogDebug("Not playing anything");
        return CommandResponse.CommandNotSuccessful("Not playing anything");
      }

      var embed = await _embedService.GenerateMusicNowPlayingEmbedAsync(lavaPlayer.Track, user, textChannel);
      if (embed == null)
      {
        _logger.LogError("Unable to generate embed");
        return CommandResponse.CommandNotSuccessful("Unable to generate embed");
      }

      return CommandResponse.CommandSuccessful(embed);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ApplyAudioFilterAsync<T>(IGuild guild, T filter)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      if (lavaPlayer.Track == null || lavaPlayer.PlayerState is None)
      {
        _logger.LogDebug("Cannot apply filter to player. Nothing is playing");
        return CommandResponse.CommandNotSuccessful("Cannot apply filter. Nothing is playing");
      }

      if (filter is IFilter audioFilter)
      {
        await lavaPlayer.ApplyFilterAsync(audioFilter);
        return CommandResponse.CommandSuccessful();
      }

      _logger.LogError("Unable to apply filter. Incorrect filter provided.");
      return CommandResponse.CommandNotSuccessful("Unable to apply filter. Wrong filter provided");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> GetLyricsFromGeniusAsync(IGuild guild)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      var lyrics = await lavaPlayer.Track.FetchLyricsFromGeniusAsync();
      if (!string.IsNullOrEmpty(lyrics))
      {
        return CommandResponse.CommandSuccessful(lyrics);
      }

      return CommandResponse.CommandNotSuccessful(
        new Exception($"Unable to get lyrics for title: {lavaPlayer.Track.Title}"));
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> GetLyricsFromOvhAsync(IGuild guild)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      if (lavaPlayer.PlayerState != Playing)
      {
        _logger.LogDebug("Cannot execute command, player is not playing");
        return CommandResponse.CommandNotSuccessful("Cannot execute command, player is not playing");
      }

      var lyrics = await lavaPlayer.Track.FetchLyricsFromOvhAsync();
      if (!string.IsNullOrEmpty(lyrics))
      {
        return CommandResponse.CommandSuccessful(lyrics);
      }

      return CommandResponse.CommandNotSuccessful(
        new Exception($"Unable to get lyrics for title: {lavaPlayer.Track.Title}"));
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse ShuffleQueue(IGuild guild)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      _logger.LogDebug(Shuffle);

      lavaPlayer.Vueue.Shuffle();

      return CommandResponse.CommandSuccessful(BotShuffleQueue);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse ToggleTwoFourSeven(IGuild guild)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      _logger.LogDebug(lavaPlayer.Is247ModeEnabled ? TwoFourSevenOff : TwoFourSevenOn);

      lavaPlayer.Toggle247Mode();

      var response = lavaPlayer.Is247ModeEnabled ? BotTwoFourSevenOn : BotTwoFourSevenOff;
      return CommandResponse.CommandSuccessful(response);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  #endregion Music Module Commands

  private async Task PlayTrack(Player<LavaTrack> lavaPlayer, SearchResponse searchResponse, IGuildUser user)
  {
    AddToPlayerQueue(lavaPlayer, searchResponse);

    // Check current player state
    if (lavaPlayer.PlayerState is Playing || (lavaPlayer.PlayerState is Paused &&
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

    // Update command author, used for Embeds
    lavaPlayer.Author = user;

    await lavaPlayer.PlayAsync(lavaTrack);
  }

  private void AddToPlayerQueue(Player<LavaTrack> lavaPlayer, SearchResponse searchResponse)
  {
    var originalQueueSize = lavaPlayer.Vueue.Count;

    if (MusicHelper.IsSearchResponsePlaylist(searchResponse))
    {
      _logger.LogDebug("Adding {TrackCount} songs to queue", searchResponse.Tracks.Count);
      lavaPlayer.Vueue.Enqueue(searchResponse.Tracks);
    }
    else
    {
      var lavaTrack = searchResponse.Tracks.Count >= 1
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
}
