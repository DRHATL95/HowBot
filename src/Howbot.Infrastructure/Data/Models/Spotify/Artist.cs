using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Spotify;

public record Artist
{
  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("href")] public string Href { get; set; } = string.Empty;

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("type")] public string Type { get; set; } = string.Empty;

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}
