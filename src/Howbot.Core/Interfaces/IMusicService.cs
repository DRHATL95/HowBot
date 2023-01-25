using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Victoria.Responses.Search;

namespace Howbot.Core.Interfaces;

public interface IMusicService : IServiceBase
{
  public Task<CommandResponse> PlayBySearchTypeAsync(SearchType searchType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel);
  public Task<CommandResponse> PauseTrackAsync(IGuild guild);
  public Task<CommandResponse> ResumeTrackAsync(IGuild guild);
  public Task<CommandResponse> SkipTrackAsync(IGuild guild, int numberOfTracks);
  public Task<CommandResponse> SeekTrackAsync(IGuild guild, TimeSpan seekPosition);
  public Task<CommandResponse> ChangeVolumeAsync(IGuild guild, int? newVolume);
  public Task<CommandResponse> NowPlayingAsync(IGuildUser user, ITextChannel textChannel);
  public Task<CommandResponse> ApplyAudioFilterAsync<T>(IGuild guild, T filter);
  public Task<CommandResponse> GetLyricsFromGeniusAsync(IGuild guild);
  public Task<CommandResponse> GetLyricsFromOvhAsync(IGuild guild);

}
