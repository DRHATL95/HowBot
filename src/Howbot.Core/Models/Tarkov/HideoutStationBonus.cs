using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class HideoutStationBonus
{
  [JsonProperty("type")] public string Type { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("value")] public float? Value { get; set; }

  [JsonProperty("passive")] public bool IsPassive { get; set; }

  [JsonProperty("production")] public bool IsProduction { get; set; }

  [JsonProperty("slotItems")] public IEnumerable<Item> SlotItems { get; set; } = [];

  [JsonProperty("skillName")] public string? SkillName { get; set; }
}
