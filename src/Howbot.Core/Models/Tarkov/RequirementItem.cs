using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class RequirementItem
{
  [JsonProperty("id")] public string? Id { get; set; }

  [JsonProperty("item")] public Item Item { get; set; } = new();

  [JsonProperty("count")] public int Count { get; set; }

  [JsonProperty("quantity")] public int Quantity { get; set; }

  [JsonProperty("attributes")] public IEnumerable<ItemAttribute>? ItemAttributes { get; set; }
}
