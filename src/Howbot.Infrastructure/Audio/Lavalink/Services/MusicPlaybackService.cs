using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink;
using Howbot.SharedKernel;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;

public class MusicPlaybackService(IAudioService audioService, IPlayerFactoryService playerFactory, ILoggerAdapter<MusicPlaybackService> logger) : IMusicPlaybackService
{
  public async ValueTask<MusicCommandResult> PlayTrackAsync(ulong guildId, ulong textChannelId, string query, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, textChannelId, ct);
      
      var track = await audioService.Tracks.LoadTrackAsync()
    }
  }

  public ValueTask<MusicCommandResult> PauseTrackAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ResumeTrackAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> SkipTrackAsync(ulong guildId, int? numberOfTracks = null, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> SeekTrackAsync(ulong guildId, TimeSpan seekPosition, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ChangeVolumeAsync(ulong guildId, int volume, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ApplyAudioFilterAsync(ulong guildId, IPlayerFilters filters, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> GetLyricsAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ToggleShuffleAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> GetQueueAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack track, string market = "US", int limit = 10,
    CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}
