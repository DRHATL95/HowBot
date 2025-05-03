using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class LootContainerPosition
{
  [JsonProperty("lootContainer")] public LootContainer LootContainer { get; set; } = new();

  [JsonProperty("position")] public MapPosition Position { get; set; } = new();
}
