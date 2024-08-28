using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class ItemAttribute
{
  [JsonProperty("type")] public string Type { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("value")] public string? Value { get; set; }
}
