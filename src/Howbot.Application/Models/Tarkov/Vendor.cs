using Newtonsoft.Json;

namespace Howbot.Application.Models.Tarkov;

public class Vendor
{
  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;
}
