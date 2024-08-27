using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public record MarketItem
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("shortName")] public string ShortName { get; set; } = string.Empty;

  [JsonProperty("basePrice")] public int BasePrice { get; set; }

  [JsonProperty("wikiLink")] public string WikiLink { get; set; } = string.Empty;

  [JsonProperty("avg24hPrice")] public int Avg24HPrice { get; set; }

  [JsonProperty("iconLink")] public string IconLink { get; set; } = string.Empty;

  [JsonProperty("updated")] public string Updated { get; set; } = string.Empty;

  [JsonProperty("sellFor")] public List<SellForRequest> SellFor { get; set; } = [];
}
