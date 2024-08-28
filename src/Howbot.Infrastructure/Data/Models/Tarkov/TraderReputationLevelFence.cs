using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class TraderReputationLevelFence
{
  [JsonProperty("minimumReputation")] public int MinimumReputation { get; set; }

  [JsonProperty("scavCooldownModifier")] public float? ScavCooldownModifier { get; set; }

  [JsonProperty("scavCaseTimeModifier")] public float? ScavCaseTimeModifier { get; set; }

  [JsonProperty("extractPriceModifier")] public float? ExtractPriceModifier { get; set; }

  [JsonProperty("scavFollowChance")] public float? ScavFollowChance { get; set; }

  [JsonProperty("scavEquipmentSpawnChanceModifier")]
  public float? ScavEquipmentSpawnChanceModifier { get; set; }

  [JsonProperty("priceModifier")] public float? PriceModifier { get; set; }

  [JsonProperty("hostileBosses")] public bool IsHostileBosses { get; set; }

  [JsonProperty("hostileScavs")] public bool IsHostileScavs { get; set; }

  [JsonProperty("scavAttackSupport")] public bool IsScavAttackSupport { get; set; }

  [JsonProperty("availableScavExtracts")]
  public int AvailableScavExtracts { get; set; }

  [JsonProperty("btrEnabled")] public bool IsBtrEnabled { get; set; }

  [JsonProperty("btrDeliveryDiscount")] public int BtrDeliveryDiscount { get; set; }

  [JsonProperty("btrDeliveryGridSize")] public MapPosition? BtrDeliveryGridSize { get; set; }

  [JsonProperty("btrTaxiDiscount")] public int BtrTaxiDiscount { get; set; }

  [JsonProperty("btrCoveringFireDiscount")]
  public int BtrCoveringFireDiscount { get; set; }
}
