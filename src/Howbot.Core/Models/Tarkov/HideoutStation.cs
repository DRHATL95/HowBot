using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class HideoutStation
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;

  [JsonProperty("imageLink")] public string? ImageUrl { get; set; }

  [JsonProperty("levels")] public IEnumerable<HideoutStationLevel> Levels { get; set; } = [];

  [JsonProperty("tarkovDataId")] public int? TarkovDataId { get; set; }

  [JsonProperty("crafts")] public IEnumerable<Craft> Crafts { get; set; } = [];
}
