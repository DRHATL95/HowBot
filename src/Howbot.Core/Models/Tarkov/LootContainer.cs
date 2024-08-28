using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class LootContainer
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;
}
