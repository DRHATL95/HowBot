using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class ItemCategory
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;

  [JsonProperty("parent")] public ItemCategory? Parent { get; set; }

  [JsonProperty("children")] public IEnumerable<ItemCategory> Children { get; set; } = [];
}
