using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class SkillLevel
{
  [JsonProperty("skill")] public Skill Skill { get; set; } = new();

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("level")] public float Level { get; set; }
}
