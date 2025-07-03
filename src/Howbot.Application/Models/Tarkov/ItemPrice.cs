using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class ItemPrice
{
  [JsonProperty("vendor")] public Vendor Vendor { get; set; } = new();

  [JsonProperty("price")] public int? Price { get; set; }

  [JsonProperty("currency")] public string? Currency { get; set; }

  [JsonProperty("currencyItem")] public Item? CurrencyItem { get; set; }

  [JsonProperty("priceRUB")] public int? PriceInRubles { get; set; }
}
