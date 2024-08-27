using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Spotify;

public record Seed
{
  [JsonProperty("initialPoolSize")] public int InitialPoolSize { get; set; }

  [JsonProperty("afterFilteringSize")] public int AfterFilteringSize { get; set; }

  [JsonProperty("afterRelinkingSize")] public int AfterRelinkingSize { get; set; }

  [JsonProperty("href")] public string Href { get; set; } = string.Empty;

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("type")] public string Type { get; set; } = string.Empty;
}
