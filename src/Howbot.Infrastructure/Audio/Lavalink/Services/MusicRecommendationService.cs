using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Interfaces.Lavalink;
using Lavalink4NET.Tracks;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;
public class MusicRecommendationService : IMusicRecommendationService
{
    public ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack track, string market = "US", int limit = 10, CancellationToken ct = default)
    {
        // Implementation for getting Spotify recommendations based on a LavalinkTrack
        throw new NotImplementedException();
    }
    public ValueTask<string> GetSpotifyRecommendationAsync(string trackId, string market = "US", int limit = 10, CancellationToken ct = default)
    {
        // Implementation for getting Spotify recommendations based on a track ID
        throw new NotImplementedException();
  }
}
