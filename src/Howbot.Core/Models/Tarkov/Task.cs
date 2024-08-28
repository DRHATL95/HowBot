using Howbot.Infrastructure.Data.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class Task
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("tarkovDataId")] public int? TarkovDataId { get; set; }

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("normalizedName")] public string NormalizedName { get; set; } = string.Empty;

  [JsonProperty("trader")] public Trader Trader { get; set; } = new();

  [JsonProperty("map")] public Map? Map { get; set; }

  [JsonProperty("experience")] public int Experience { get; set; }

  [JsonProperty("wikiLink")] public string? WikiLink { get; set; }

  [JsonProperty("taskImageLink")] public string? TaskImageLink { get; set; }

  [JsonProperty("minPlayerLevel")] public int MinPlayerLevel { get; set; }

  [JsonProperty("taskRequirements")] public IEnumerable<TaskStatusRequirement> TaskRequirements { get; set; } = [];

  [JsonProperty("traderRequirements")] public IEnumerable<RequirementTrader> TraderRequirements { get; set; } = [];

  [JsonProperty("objectives")] public IEnumerable<TaskObjective> Objectives { get; set; } = [];

  [JsonProperty("startRewards")] public TaskReward? StartRewards { get; set; }

  [JsonProperty("finishRewards")] public TaskReward? FinishRewards { get; set; }

  [JsonProperty("failConditions")] public IEnumerable<TaskObjective> FailConditions { get; set; } = [];

  [JsonProperty("failureOutcome")] public TaskReward? FailureOutcome { get; set; }

  [JsonProperty("restartable")] public bool IsRestartable { get; set; }

  [JsonProperty("factionName")] public string? FactionName { get; set; }

  [JsonProperty("kappaRequired")] public bool IsKappaRequired { get; set; }

  [JsonProperty("lightkeeperRequired")] public bool IsLightKeeperRequired { get; set; }

  [JsonProperty("descriptionMessageId")] public string? DescriptionMessageId { get; set; }

  [JsonProperty("startMessageId")] public string? StartMessageId { get; set; }

  [JsonProperty("successMessageId")] public string? SuccessMessageId { get; set; }

  [JsonProperty("failMessageId")] public string? FailMessageId { get; set; }
}
