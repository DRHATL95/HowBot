using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class RequirementHideoutStationLevel
{
  [JsonProperty("id")] public string? Id { get; set; }

  [JsonProperty("station")] public HideoutStation Station { get; set; } = new();

  [JsonProperty("level")] public int Level { get; set; }
}
