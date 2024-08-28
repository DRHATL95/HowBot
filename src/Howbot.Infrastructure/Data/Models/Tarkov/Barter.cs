using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class Barter
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("trader")] public Trader Trader { get; set; } = new();

  [JsonProperty("level")] public int Level { get; set; }

  [JsonProperty("taskUnlock")] public Task? TaskUnlock { get; set; }

  [JsonProperty("requiredItems")] public IEnumerable<ContainedItem> RequiredItems { get; set; } = [];

  [JsonProperty("rewardItems")] public IEnumerable<ContainedItem> RewardItems { get; set; } = [];

  [JsonProperty("buyLimit")] public int? BuyLimit { get; set; }
}
