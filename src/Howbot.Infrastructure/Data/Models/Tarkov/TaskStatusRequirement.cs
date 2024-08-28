using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class TaskStatusRequirement
{
  [JsonProperty("task")] public Task Task { get; set; } = new();

  [JsonProperty("status")] public IEnumerable<string> Statuses { get; set; } = [];
}
