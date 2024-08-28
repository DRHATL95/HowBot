using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class MapHazard
{
  [JsonProperty("hazardType")] public string HazardType { get; set; } = string.Empty;

  [JsonProperty("name")] public string? Name { get; set; }

  [JsonProperty("position")] public MapPosition Position { get; set; } = new();

  [JsonProperty("outline")] public IEnumerable<MapPosition> Outline { get; set; } = [];

  [JsonProperty("top")] public float? Top { get; set; }

  [JsonProperty("bottom")] public float? Bottom { get; set; }
}
