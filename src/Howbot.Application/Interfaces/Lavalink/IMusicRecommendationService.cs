using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Tracks;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IMusicRecommendationService
{
  ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack track, string market = "US", int limit = 10, CancellationToken ct = default);
  ValueTask<string> GetSpotifyRecommendationAsync(string trackId, string market = "US", int limit = 10, CancellationToken ct = default);
}
