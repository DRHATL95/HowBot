using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class TraderLevel
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("level")] public int Level { get; set; }

  [JsonProperty("requiredPlayerLevel")] public int RequiredPlayerLevel { get; set; }

  [JsonProperty("requiredReputation")] public int RequiredReputation { get; set; }

  [JsonProperty("requiredCommerce")] public int RequiredCommerce { get; set; }

  [JsonProperty("payRate")] public float PayRate { get; set; }

  [JsonProperty("insuranceRate")] public float? InsuranceRate { get; set; }

  [JsonProperty("repairCostMultiplier")] public float? RepairCostMultiplier { get; set; }

  [JsonProperty("barters")] public IEnumerable<Barter> Barters { get; set; } = [];

  [JsonProperty("cashOffers")] public IEnumerable<TradeCashOffer> CashOffers { get; set; } = [];

  [JsonProperty("imageLink")] public string? ImageUrl { get; set; }

  [JsonProperty("image4xLink")] public string? Image4XUrl { get; set; }
}
