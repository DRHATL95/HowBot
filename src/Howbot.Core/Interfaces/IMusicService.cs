using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;

namespace Howbot.Core.Interfaces;

public interface IMusicService : IServiceBase
{
  [NotNull]
  public Task<CommandResponse> PlayTrackBySearchTypeAsync<T>(T player, SearchProviderTypes searchProviderType, [NotNull] string searchRequest, [NotNull] IGuildUser user,
    [NotNull] IVoiceState voiceState, [NotNull] ITextChannel textChannel) where T : ILavalinkPlayer;

  [NotNull]
  public Task<CommandResponse> PauseTrackAsync<T>(T player) where T : ILavalinkPlayer;

  [NotNull]
  public Task<CommandResponse> ResumeTrackAsync<T>(T player) where T : ILavalinkPlayer;

  [NotNull]
  public Task<CommandResponse> SkipTrackAsync<T>(T player, int numberOfTracks) where T : ILavalinkPlayer;

  [NotNull]
  public Task<CommandResponse> SeekTrackAsync<T>(T player, TimeSpan seekPosition) where T : ILavalinkPlayer;

  [NotNull]
  public Task<CommandResponse> ChangeVolumeAsync<T>(T player, [CanBeNull] int? newVolume) where T : ILavalinkPlayer;

  public Task<CommandResponse> NowPlayingAsync<T>(T player, [NotNull] IGuildUser user, [NotNull] ITextChannel textChannel) where T : ILavalinkPlayer;

  public Task<CommandResponse> ApplyAudioFilterAsync<T>(T player, [NotNull] IPlayerFilters filter) where T : ILavalinkPlayer;

  public Task<CommandResponse> GetLyricsFromTrackAsync<T>(T player) where T : ILavalinkPlayer;

  public CommandResponse ToggleShuffle<T>(T player) where T : ILavalinkPlayer;

  /*  public CommandResponse ToggleTwoFourSeven(); */

  /* public Task<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1); */
}
