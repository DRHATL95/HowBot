using Newtonsoft.Json;

namespace Howbot.Core.Models;

public class GuildDto
{
  [JsonProperty("id")] public ulong Id { get; set; }

  [JsonProperty("name")] public string? Name { get; set; }

  [JsonProperty("icon")] public string? Icon { get; set; }

  [JsonProperty("owner")] public bool Owner { get; set; }

  [JsonProperty("permissions")] public int Permissions { get; set; }
}
