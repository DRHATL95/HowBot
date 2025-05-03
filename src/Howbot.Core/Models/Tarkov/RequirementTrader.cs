using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class RequirementTrader
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("trader")] public Trader Trader { get; set; } = new();

  [JsonProperty("requirementType")] public string? RequirementType { get; set; }

  [JsonProperty("compareMethod")] public string? CompareMethod { get; set; }

  [JsonProperty("value")] public int Value { get; set; }
}
