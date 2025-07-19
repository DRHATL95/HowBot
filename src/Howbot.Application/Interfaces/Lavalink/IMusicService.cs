using System.Collections.Immutable;
using Discord;
using Discord.Interactions;
using Howbot.Application.Models.Discord.Commands;
using Howbot.Application.Models.Lavalink;
using Howbot.Application.Models.Lavalink.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Tracks;

namespace Howbot.Application.Interfaces.Lavalink;

public interface IMusicService
{
  void Initialize();

  ValueTask<string> GetSessionIdForGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);

  ValueTask<MusicCommandResult> PlayTrackBySearchTypeAsync(HowbotPlayer player, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel);

  ValueTask<MusicCommandResult> PauseTrackAsync(HowbotPlayer player);

  ValueTask<MusicCommandResult> ResumeTrackAsync(HowbotPlayer player);

  ValueTask<MusicCommandResult> SkipTrackAsync(HowbotPlayer player, int? numberOfTracks);

  ValueTask<MusicCommandResult> SeekTrackAsync(HowbotPlayer player, TimeSpan seekPosition);

  ValueTask<MusicCommandResult> ChangeVolumeAsync(HowbotPlayer player, int newVolume);

  CommandResponse NowPlaying(HowbotPlayer player, IGuildUser user,
    ITextChannel textChannel);

  ValueTask<MusicCommandResult> ApplyAudioFilterAsync(HowbotPlayer player, IPlayerFilters filter);

  ValueTask<MusicCommandResult> GetLyricsFromTrackAsync(HowbotPlayer player);

  CommandResponse ToggleShuffle(HowbotPlayer player);

  ValueTask<HowbotPlayer?> GetPlayerByContextAsync(
    SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true,
    ImmutableArray<IPlayerPrecondition> preconditions = default, bool isDeferred = false, int initialVolume = 100,
    CancellationToken cancellationToken = default);

  ValueTask<MusicCommandResult> JoinVoiceChannelAsync(ulong guildId, ulong voiceChannelId,
    CancellationToken cancellationToken = default);

  MusicCommandResult GetMusicQueueForServer(HowbotPlayer player);

  ValueTask<HowbotPlayer?> GetPlayerByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);

  ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack lavalinkTrack, string market = "US", int limit = 10,
    CancellationToken cancellationToken = default);
}
