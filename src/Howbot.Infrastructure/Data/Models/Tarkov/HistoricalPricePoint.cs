﻿using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class HistoricalPricePoint
{
  [JsonProperty("price")] public int? Price { get; set; }

  [JsonProperty("priceMin")] public int? PriceMinimum { get; set; }

  [JsonProperty("timestamp")] public string? Timestamp { get; set; }
}
