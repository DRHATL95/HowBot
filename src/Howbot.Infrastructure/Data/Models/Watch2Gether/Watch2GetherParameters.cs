using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Watch2Gether;

public record Watch2GetherParameters
{
  [JsonProperty("w2g_api_key")] public string W2GApiKey { get; set; } = string.Empty;

  [JsonProperty("share")] public string Share { get; set; } = string.Empty;

  [JsonProperty("bg_color")] public string BackgroundColor { get; set; } = string.Empty;

  [JsonProperty("bg_opacity")] public string BackgroundOpacity { get; set; } = string.Empty;
}
