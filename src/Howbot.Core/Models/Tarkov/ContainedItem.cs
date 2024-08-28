using Howbot.Infrastructure.Data.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class ContainedItem
{
  [JsonProperty("item")] public Item Item { get; set; } = new();

  [JsonProperty("count")] public float Count { get; set; }

  [JsonProperty("quantity")] public float Quantity { get; set; }

  [JsonProperty("attributes")] public IEnumerable<ItemAttribute>? ItemAttributes { get; set; }
}
