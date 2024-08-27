using Howbot.Infrastructure.Services;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Spotify;

public record Track
{
  [JsonProperty("album")] public Album Album { get; set; } = new();

  [JsonProperty("artists")] public List<Artist> Artists { get; set; } = new();

  [JsonProperty("duration_ms")] public int DurationMs { get; set; }

  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("popularity")] public int Popularity { get; set; }

  [JsonProperty("preview_url")] public string PreviewUrl { get; set; } = string.Empty;

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}
