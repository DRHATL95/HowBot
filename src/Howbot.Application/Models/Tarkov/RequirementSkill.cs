using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class RequirementSkill
{
  [JsonProperty("id")] public string? Id { get; set; }

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("skill")] public Skill Skill { get; set; } = new();

  [JsonProperty("level")] public int Level { get; set; }
}
