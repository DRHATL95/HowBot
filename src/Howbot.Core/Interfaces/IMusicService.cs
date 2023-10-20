using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using JetBrains.Annotations;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for the MusicService
public interface IMusicService
{
  /// <summary>
  ///   Initializes the MusicService
  /// </summary>
  void Initialize();

  /// <summary>
  ///   Searches for a track by the given search provider type and search request and plays in requested voice channel for
  ///   guild.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="searchProviderType"></param>
  /// <param name="searchRequest"></param>
  /// <param name="user"></param>
  /// <param name="voiceState"></param>
  /// <param name="textChannel"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> PlayTrackBySearchTypeAsync([NotNull] HowbotPlayer player,
    SearchProviderTypes searchProviderType, [NotNull] string searchRequest, [NotNull] IGuildUser user,
    [NotNull] IVoiceState voiceState, [NotNull] ITextChannel textChannel);

  /// <summary>
  ///   Pauses the current track playing in the requested guild.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> PauseTrackAsync([NotNull] HowbotPlayer player);

  /// <summary>
  ///   Resumes the current track paused in the requested guild.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> ResumeTrackAsync([NotNull] HowbotPlayer player);

  /// <summary>
  ///   Skips the current track playing or the number of tracks requested in the guild.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="numberOfTracks"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> SkipTrackAsync([NotNull] HowbotPlayer player, [CanBeNull] int? numberOfTracks);

  /// <summary>
  ///   Attempts to seek on the current playing track.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="seekPosition"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> SeekTrackAsync([NotNull] HowbotPlayer player, TimeSpan seekPosition);

  /// <summary>
  ///   Attempts to change the current volume of the player.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="newVolume"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> ChangeVolumeAsync([NotNull] HowbotPlayer player, int newVolume);

  /// <summary>
  ///   Generates an embed for the current track playing for the guild.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="user"></param>
  /// <param name="textChannel"></param>
  /// <returns></returns>
  CommandResponse NowPlaying([NotNull] HowbotPlayer player, [NotNull] IGuildUser user,
    [NotNull] ITextChannel textChannel);

  /// <summary>
  ///   Applies audio filter the current track playing for the guild.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="filter"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> ApplyAudioFilterAsync([NotNull] HowbotPlayer player, [NotNull] IPlayerFilters filter);

  /// <summary>
  ///   Creates an embed of lyrics for the current track playing for the guild.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> GetLyricsFromTrackAsync([NotNull] HowbotPlayer player);

  /// <summary>
  ///   Toggles shuffles on the current playing player for the guild.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  CommandResponse ToggleShuffle([NotNull] HowbotPlayer player);

  /// <summary>
  ///   Get the player for the command context guild. If the player does not exist, it will be created.
  /// </summary>
  /// <param name="context"></param>
  /// <param name="allowConnect"></param>
  /// <param name="requireChannel"></param>
  /// <param name="preconditions"></param>
  /// <param name="isDeferred"></param>
  /// <param name="initialVolume"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  ValueTask<HowbotPlayer> GetPlayerByContextAsync(
    [NotNull] SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true,
    ImmutableArray<IPlayerPrecondition> preconditions = default, bool isDeferred = false, int initialVolume = 100,
    CancellationToken cancellationToken = default);

  /// <summary>
  ///   Toggles the current playing player to consistently play songs related to the current playing track. Can be used as a
  ///   radio feature.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  CommandResponse ToggleTwoFourSeven([NotNull] HowbotPlayer player);

  /// <summary>
  ///   Calls YouTube API to get recommended video id for the given video id.
  /// </summary>
  /// <param name="videoId"></param>
  /// <param name="count"></param>
  /// <returns></returns>
  ValueTask<IEnumerable<string>> GetYoutubeRecommendedVideoId([NotNull] string videoId, int count = 1);
}
