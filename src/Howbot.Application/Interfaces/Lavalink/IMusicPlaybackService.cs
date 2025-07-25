using Howbot.Application.Models.Lavalink;
using Lavalink4NET.Players;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IMusicPlaybackService
{
  ValueTask<MusicCommandResult> PlayTrackAsync(ulong guildId, ulong voiceChannelId, string query, CancellationToken ct = default);
  ValueTask<MusicCommandResult> PauseTrackAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ResumeTrackAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default);
  ValueTask<MusicCommandResult> SkipTrackAsync(ulong guildId, ulong voiceChannelId, int? numberOfTracks = null, CancellationToken ct = default);
  ValueTask<MusicCommandResult> SeekTrackAsync(ulong guildId, ulong voiceChannelId, TimeSpan seekPosition, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ChangeVolumeAsync(ulong guildId, ulong voiceChannelId, int volume, CancellationToken ct = default);
  ValueTask<MusicCommandResult> ApplyAudioFilterAsync(ulong guildId, ulong voiceChannelId, IPlayerFilters filters, CancellationToken ct = default);
  ValueTask<MusicCommandResult> GetLyricsAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default);
}
