using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class Map
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("tarkovDataId")] public string? TarkovDataId { get; set; }

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;

  [JsonProperty("wiki")] public string? Wiki { get; set; }

  [JsonProperty("description")] public string? Description { get; set; }

  [JsonProperty("enemies")] public IEnumerable<string> Enemies { get; set; } = [];

  [JsonProperty("raidDuration")] public int RaidDuration { get; set; }

  [JsonProperty("players")] public string Players { get; set; } = string.Empty;

  [JsonProperty("bosses")] public IEnumerable<BossSpawn> Bosses { get; set; } = [];

  [JsonProperty("nameId")] public string? NameId { get; set; }

  [JsonProperty("accessKeys")] public IEnumerable<Item> AccessKeys { get; set; } = [];

  [JsonProperty("accessKeysMinPlayerLevel")]
  public int? AccessKeysMinPlayerLevel { get; set; }

  [JsonProperty("minPlayerLevel")] public int? MinPlayerLevel { get; set; }

  [JsonProperty("maxPlayerLevel")] public int? MaxPlayerLevel { get; set; }

  [JsonProperty("spawns")] public IEnumerable<MapSpawn> Spawns { get; set; } = [];

  [JsonProperty("extracts")] public IEnumerable<MapExtract> Extracts { get; set; } = [];

  [JsonProperty("transits")] public IEnumerable<MapTransit> Transits { get; set; } = [];

  [JsonProperty("locks")] public IEnumerable<Lock> Locks { get; set; } = [];

  [JsonProperty("switches")] public IEnumerable<MapSwitch> Switches { get; set; } = [];

  [JsonProperty("hazards")] public IEnumerable<MapHazard> Hazards { get; set; } = [];

  [JsonProperty("lootContainers")] public IEnumerable<LootContainerPosition> LootContainers { get; set; } = [];

  [JsonProperty("stationaryWeapons")] public IEnumerable<StationaryWeaponPosition> StationaryWeapons { get; set; } = [];
}
