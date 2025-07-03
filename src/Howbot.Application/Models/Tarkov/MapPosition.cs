using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class MapPosition
{
  [JsonProperty("x")] public float X { get; set; }

  [JsonProperty("y")] public float Y { get; set; }

  [JsonProperty("z")] public float Z { get; set; }
}
