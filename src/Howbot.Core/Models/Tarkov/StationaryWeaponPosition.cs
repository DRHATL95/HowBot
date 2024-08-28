using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class StationaryWeaponPosition
{
  [JsonProperty("stationaryWeapon")] public StationaryWeapon StationaryWeapon { get; set; } = new();

  [JsonProperty("position")] public MapPosition Position { get; set; } = new();
}
