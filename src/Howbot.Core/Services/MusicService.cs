using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Serilog;

namespace Howbot.Core.Services;

public class MusicService : ServiceBase<MusicService>, IMusicService
{

  [NotNull] private readonly IEmbedService _embedService;
  
  [NotNull] private readonly ILoggerAdapter<MusicService> _logger;

  [NotNull] private readonly IAudioService _audioService;

  [NotNull] private readonly ILyricsService _lyricsService;

  public MusicService([NotNull] IEmbedService embedService, [NotNull] IAudioService audioService, [NotNull] ILoggerAdapter<MusicService> logger, [NotNull] ILyricsService lyricsService) : base(logger)
  {
    _embedService = embedService;
    _logger = logger;
    _lyricsService = lyricsService;
    _audioService = audioService;
  }

  #region Music Module Commands

  public async Task<CommandResponse> PlayTrackBySearchTypeAsync<T>(T player, SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel) where T : ILavalinkPlayer
  {
    try
    {
      var type = ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);

      // This is using Lavalink4Net.Extensions.LavaSearch - CURRENTLY NOT WORKING (Only Texts is being populated)
      /*var searchResponse = await _audioService.Tracks.SearchAsync(query: searchRequest,
        loadOptions: new TrackLoadOptions(SearchMode: type),
        categories: ImmutableArray.Create(SearchCategory.Track)).ConfigureAwait(false);*/

      var track = await _audioService.Tracks.LoadTrackAsync(searchRequest, type);

      if (track is null) return CommandResponse.CommandNotSuccessful("Unable to find any tracks");

      await player.PlayAsync(track).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful(track);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> PauseTrackAsync<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      _logger.LogDebug("Pausing current track [{GuildId}]", player.GuildId);

      await player.PauseAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ResumeTrackAsync<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      if (player.State is not PlayerState.Paused)
      {
        return CommandResponse.CommandNotSuccessful("Player is not paused.");
      }

      await player.ResumeAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful();

    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(ResumeTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SkipTrackAsync<T>(T player, int numberOfTracks) where T : ILavalinkPlayer
  {
    try
    {
      if (player is IQueuedLavalinkPlayer { State: PlayerState.Playing } queuedPlayer)
      {
        await queuedPlayer.SkipAsync(numberOfTracks);

        return CommandResponse.CommandSuccessful($"Skipped {numberOfTracks} tracks in queue.");
      }

      return CommandResponse.CommandNotSuccessful("Unable to skip to position in queue.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.SkipTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SeekTrackAsync<T>(T player, TimeSpan seekPosition) where T : ILavalinkPlayer
  {
    try
    {
      _logger.LogDebug($"Seeking to {seekPosition:g}.");

      await player.SeekAsync(seekPosition).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful($"Seeking to {seekPosition:g}.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ChangeVolumeAsync<T>(T player, int? newVolume) where T : ILavalinkPlayer
  {
    try
    {
      // Since second param is optional, if 0 do nothing but return command success
      if (!newVolume.HasValue)
      {
        return CommandResponse.CommandSuccessful();
      }

      if (player is not QueuedLavalinkPlayer queuedLavalinkPlayer)
      {
        return CommandResponse.CommandNotSuccessful("Unable to change volume.");
      }

      await queuedLavalinkPlayer.SetVolumeAsync(newVolume.Value / 100f).ConfigureAwait(false);
      return CommandResponse.CommandSuccessful($"Volume set to {newVolume}%");

    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> NowPlayingAsync<T>(T player, IGuildUser user, ITextChannel textChannel) where T : ILavalinkPlayer
  {
    try
    {
      if (player.CurrentTrack is null)
      {
        return CommandResponse.CommandNotSuccessful("No track is currently playing.");
      }

      var embed = await _embedService.GenerateMusicNowPlayingEmbedAsync(player.CurrentTrack, user, textChannel, player.Position?.Position);

      return CommandResponse.CommandSuccessful(embed);

    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public Task<CommandResponse> ApplyAudioFilterAsync<T>(T player, IPlayerFilters filter) where T : ILavalinkPlayer
  {
    try
    {
      // Create filter options
      /*var options = new EchoFilterOptions
      {
        Delay = 1.0F,
      };

      player.Filters.Echo(options);*/

      return Task.FromResult(CommandResponse.CommandSuccessful());
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return Task.FromResult(CommandResponse.CommandNotSuccessful(exception));
    }
  }

  public async Task<CommandResponse> GetLyricsFromTrackAsync<T>(T player) where T: ILavalinkPlayer
  {
    try
    {
      var track = player.CurrentTrack;

      if (track is null)
      {
        return CommandResponse.CommandNotSuccessful("Nothing is playing.");
      }

      var lyrics = await _lyricsService.GetLyricsAsync(track.Author, track.Title);

      return string.IsNullOrEmpty(lyrics) ? CommandResponse.CommandNotSuccessful("No lyrics found for current song playing.") : CommandResponse.CommandSuccessful(lyrics);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse ToggleShuffle<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      if (player is not IQueuedLavalinkPlayer queuedLavalinkPlayer)
      {
        return CommandResponse.CommandNotSuccessful("Unable to shuffle queue.");
      }

      queuedLavalinkPlayer.Shuffle = !queuedLavalinkPlayer.Shuffle;

      return CommandResponse.CommandSuccessful($"Shuffle is now {(queuedLavalinkPlayer.Shuffle ? "enabled" : "disabled")}.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }


  /*public CommandResponse ToggleTwoFourSeven(IGuild guild)
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
  }*/

  #endregion Music Module Commands

  private static ValueTask<HowbotPlayer> CreatePlayerAsync(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    ArgumentNullException.ThrowIfNull(properties);

    Log.Logger.Information("Creating new player..");

    return ValueTask.FromResult(new HowbotPlayer(properties));
  } 
  
  [ItemCanBeNull]
  public async ValueTask<IQueuedLavalinkPlayer> GetPlayerByContextAsync(SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default, bool isDeferred = false,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    // Can't execute function without socket context
    ArgumentNullException.ThrowIfNull(context, nameof(context));

    if (context.User is not SocketGuildUser guildUser)
    {
      await context.Interaction.FollowupAsync("Unable to create player, command requested by non-guild member.")
        .ConfigureAwait(false);

      return null;
    }

    ulong voiceChannelId = guildUser.VoiceChannel?.Id ?? 0;
    IGuild guild = context.Guild;
    ulong guildId = guild.Id;

    var retrieveOptions = new PlayerRetrieveOptions(
      ChannelBehavior: allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
      VoiceStateBehavior: requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
      Preconditions: preconditions);

    HowbotPlayerOptions playerOptions = new HowbotPlayerOptions()
    {
      DisconnectOnDestroy = true,
      DisconnectOnStop = true,
      SelfDeaf = true,
      ClearQueueOnStop = true,
      ClearHistoryOnStop = true
    };

    var result = await _audioService.Players.RetrieveAsync<HowbotPlayer, HowbotPlayerOptions>(guildId, voiceChannelId, CreatePlayerAsync,
        retrieveOptions: retrieveOptions, options: new OptionsWrapper<HowbotPlayerOptions>(playerOptions), cancellationToken: cancellationToken)
      .ConfigureAwait(false);

    if (!result.IsSuccess)
    {
      var errorMessage = result.Status switch
      {
        // The user is not connected to a voice channel.
        PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",

        // The bot is not in a voice channel
        PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected to a voice channel.",

        // The bot is not in the same voice channel as the user.
        PlayerRetrieveStatus.VoiceChannelMismatch => "You are not in the same voice channel as the bot.",

        // The bot failed it's precondition check.
        PlayerRetrieveStatus.PreconditionFailed => "The bot failed it's precondition check.",

        _ => "An unknown error occurred while creating the player."
      };

      await context.Interaction.FollowupAsync(errorMessage).ConfigureAwait(false);

      return null;
    }

    return result.Player;
  }

  private static TrackSearchMode ConvertSearchProviderTypeToTrackSearchMode(SearchProviderTypes searchProviderType)
  {
    return searchProviderType switch
    {
      SearchProviderTypes.Apple => TrackSearchMode.AppleMusic,
      SearchProviderTypes.Deezer => TrackSearchMode.Deezer,
      SearchProviderTypes.SoundCloud => TrackSearchMode.SoundCloud,
      SearchProviderTypes.Spotify => TrackSearchMode.Spotify,
      SearchProviderTypes.YouTube => TrackSearchMode.YouTube,
      SearchProviderTypes.YouTubeMusic => TrackSearchMode.YouTubeMusic,
      SearchProviderTypes.YandexMusic => TrackSearchMode.YandexMusic,
      _ => TrackSearchMode.YouTube
    };
  }

  /*public async Task<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1)
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
  }*/

}
