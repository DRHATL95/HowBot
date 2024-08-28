using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class HideoutStationLevel
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("level")] public int Level { get; set; }

  [JsonProperty("constructionTime")] public int ConstructionTime { get; set; }

  [JsonProperty("description")] public string Description { get; set; } = string.Empty;

  [JsonProperty("itemRequirements")] public IEnumerable<RequirementItem> ItemRequirements { get; set; } = [];

  [JsonProperty("stationLevelRequirements")]
  public IEnumerable<RequirementHideoutStationLevel> StationLevelRequirements { get; set; } = [];

  [JsonProperty("skillRequirements")] public IEnumerable<RequirementSkill> SkillRequirements { get; set; } = [];

  [JsonProperty("traderRequirements")] public IEnumerable<RequirementTrader> TraderRequirements { get; set; } = [];

  [JsonProperty("tarkovDataId")] public int? TarkovDataId { get; set; }

  // Crafts is only available via the hideoutStations query.
  [JsonProperty("crafts")] public IEnumerable<Craft> Crafts { get; set; } = [];

  [JsonProperty("bonuses")] public IEnumerable<HideoutStationBonus> Bonuses { get; set; } = [];
}
