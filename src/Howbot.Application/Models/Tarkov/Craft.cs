using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class Craft
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("station")] public HideoutStation Station { get; set; } = new();

  [JsonProperty("level")] public int Level { get; set; }

  [JsonProperty("taskUnlock")] public Task? TaskUnlock { get; set; }

  [JsonProperty("duration")] public int Duration { get; set; }

  [JsonProperty("requiredItems")] public IEnumerable<ContainedItem> RequiredItems { get; set; } = [];

  [JsonProperty("requiredQuestItems")] public IEnumerable<QuestItem> RequiredQuestItems { get; set; } = [];

  [JsonProperty("rewardItems")] public IEnumerable<ContainedItem> RewardItems { get; set; } = [];
}
