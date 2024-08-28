using Howbot.Core.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class Trader
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;

  [JsonProperty("description")] public string? Description { get; set; }

  [JsonProperty("resetTime")] public string? ResetTime { get; set; }

  [JsonProperty("currency")] public Item Currency { get; set; } = new();

  [JsonProperty("discount")] public float Discount { get; set; }

  [JsonProperty("levels")] public IEnumerable<TraderLevel> TraderLevels { get; set; } = [];

  [JsonProperty("reputationLevels")]
  public IEnumerable<TraderReputationLevel> TraderReputationLevels { get; set; } = [];

  [JsonProperty("barters")] public IEnumerable<Barter> Barters { get; set; } = [];

  [JsonProperty("cashOffers")] public IEnumerable<TradeCashOffer> TradeCashOffers { get; set; } = [];

  [JsonProperty("imageLink")] public string? ImageUrl { get; set; }

  [JsonProperty("image4xLink")] public string? Image4XUrl { get; set; }

  [JsonProperty("tarkovDataId")] public int TarkovDataId { get; set; }
}
