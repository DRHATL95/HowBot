using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Spotify;

public record Image
{
  [JsonProperty("height")] public int Height { get; set; }

  [JsonProperty("url")] public string Url { get; set; } = string.Empty;

  [JsonProperty("width")] public int Width { get; set; }
}
