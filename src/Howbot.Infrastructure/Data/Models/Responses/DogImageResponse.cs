using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Responses;

public class DogImageResponse
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("url")] public string Url { get; set; } = string.Empty;

  [JsonProperty("width")] public int Width { get; set; }

  [JsonProperty("height")] public int Height { get; set; }
}
