using Howbot.Infrastructure.Data.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Responses;

public record EftMarketResponse
{
  [JsonProperty("data")] public EftMarketResponseData Data { get; set; } = new();
}

public record EftMarketResponseData
{
  [JsonProperty("items")] public IEnumerable<MarketItem> Items { get; set; } = [];
}



