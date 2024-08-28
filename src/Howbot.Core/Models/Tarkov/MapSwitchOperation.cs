using Howbot.Core.Interfaces.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class MapSwitchOperation
{
  [JsonProperty("operation")] public string? Operation { get; set; }

  [JsonProperty("target")] public IMapSwitchTarget? Target { get; set; }
}
