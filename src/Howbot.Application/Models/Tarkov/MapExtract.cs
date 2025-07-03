using Howbot.Application.Interfaces.Tarkov;

namespace Howbot.Application.Models.Tarkov;

public class MapExtract : IMapSwitchTarget
{
  public string? Faction { get; set; }

  public IEnumerable<MapSwitch> MapSwitches { get; set; } = [];

  public IEnumerable<MapPosition> MapOutline { get; set; } = [];

  public float? Top { get; set; }

  public float? Bottom { get; set; }
  public string Id { get; set; } = string.Empty;

  public string? Name { get; set; }

  public MapPosition? MapPosition { get; set; }
}
