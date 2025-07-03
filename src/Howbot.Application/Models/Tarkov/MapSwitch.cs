using Howbot.Application.Interfaces.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class MapSwitch : IMapSwitchTarget
{
  [JsonProperty("switchType")] public string? SwitchType { get; set; }

  [JsonProperty("activatedBy")] public MapSwitch? ActivatedBy { get; set; }

  [JsonProperty("activates")] public IEnumerable<MapSwitchOperation> Activates { get; set; } = [];

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string? Name { get; set; }

  [JsonProperty("position")] public MapPosition? MapPosition { get; set; }
}
