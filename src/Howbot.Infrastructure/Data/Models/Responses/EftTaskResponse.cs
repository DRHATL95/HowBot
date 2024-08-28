using Newtonsoft.Json;
using Task = Howbot.Core.Models.Tarkov.Task;

namespace Howbot.Infrastructure.Data.Models.Responses;

public class EftTaskResponse
{
  [JsonProperty("data")] public EftTaskResponseData Data { get; set; } = new();
}

public class EftTaskResponseData
{
  [JsonProperty("tasks")] public IEnumerable<Task> Tasks { get; set; } = [];
}
