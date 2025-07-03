using Howbot.Application.Interfaces.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class MapSwitchOperation
{
  [JsonProperty("operation")] public string? Operation { get; set; }

  [JsonProperty("target")] public IMapSwitchTarget? Target { get; set; }
}
