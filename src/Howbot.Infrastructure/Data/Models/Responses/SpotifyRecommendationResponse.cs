using Howbot.Infrastructure.Data.Models.Spotify;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Responses;

public class SpotifyRecommendationResponse
{
  [JsonProperty("tracks")] public List<Track> Tracks { get; set; } = new();

  [JsonProperty("seeds")] public List<Seed> Seeds { get; set; } = new();
}
