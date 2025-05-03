using Howbot.Core.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Responses;

public record EftMarketResponse
{
  [JsonProperty("data")] public EftMarketResponseData Data { get; set; } = new();
}

public record EftMarketResponseData
{
  [JsonProperty("items")] public IEnumerable<Item> Items { get; set; } = [];
}
