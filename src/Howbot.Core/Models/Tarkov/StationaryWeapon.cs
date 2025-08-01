﻿using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class StationaryWeapon
{
  [JsonProperty("id")] public string? Id { get; set; }

  [JsonProperty("name")] public string? Name { get; set; }

  [JsonProperty("shortName")] public string? ShortName { get; set; }
}
