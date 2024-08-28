using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public record SellForRequest
{
  [JsonProperty("price")] public int Price { get; set; }

  [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;

  [JsonProperty("priceRUB")] public int PriceInRubles { get; set; }

  [JsonProperty("source")] public string Source { get; set; } = string.Empty;
}
