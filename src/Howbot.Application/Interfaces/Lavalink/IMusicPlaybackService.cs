using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Models.Lavalink;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IMusicPlaybackService
{
  ValueTask<MusicCommandResult> PlayTrackAsync(ulong guildId, ulong voiceChannelId, string query, CancellationToken ct = default);
  ValueTask<MusicCommandResult> PauseTrackAsync(ulong guildId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ResumeTrackAsync(ulong guildId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> SkipTrackAsync(ulong guildId, int? numberOfTracks = null, CancellationToken ct = default);
  ValueTask<MusicCommandResult> SeekTrackAsync(ulong guildId, TimeSpan seekPosition, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ChangeVolumeAsync(ulong guildId, int volume, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ApplyAudioFilterAsync(ulong guildId, IPlayerFilters filters, CancellationToken ct = default);
  ValueTask<MusicCommandResult> GetLyricsAsync(ulong guildId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ToggleShuffleAsync(ulong guildId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> GetQueueAsync(ulong guildId, CancellationToken ct = default);
  ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack track, string market = "US", int limit = 10, CancellationToken ct = default);
}
