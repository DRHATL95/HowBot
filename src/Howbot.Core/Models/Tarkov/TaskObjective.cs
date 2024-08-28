using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class TaskObjective
{
  [JsonProperty("id")] public string? Id { get; set; }

  [JsonProperty("type")] public string Type { get; set; } = string.Empty;

  [JsonProperty("description")] public string Description { get; set; } = string.Empty;

  [JsonProperty("maps")] public IEnumerable<Map> Maps { get; set; } = [];

  [JsonProperty("optional")] public bool IsOptional { get; set; }
}
