using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class TaskReward
{
  [JsonProperty("traderStanding")] public IEnumerable<TraderStanding> TraderStanding { get; set; } = [];

  [JsonProperty("items")] public IEnumerable<ContainedItem> Items { get; set; } = [];

  [JsonProperty("offerUnlock")] public IEnumerable<OfferUnlock> OfferUnlock { get; set; } = [];

  [JsonProperty("skillLevelReward")] public IEnumerable<SkillLevel> SkillLevelReward { get; set; } = [];

  [JsonProperty("traderUnlock")] public IEnumerable<TraderUnlock> TraderUnlock { get; set; } = [];

  [JsonProperty("craftUnlock")] public IEnumerable<CraftUnlock> CraftUnlock { get; set; } = [];
}
