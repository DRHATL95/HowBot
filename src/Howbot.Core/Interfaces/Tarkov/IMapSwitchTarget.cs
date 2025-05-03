using Howbot.Core.Models.Tarkov;

namespace Howbot.Core.Interfaces.Tarkov;

public interface IMapSwitchTarget
{
  public string Id { get; set; }

  public string? Name { get; set; }

  public MapPosition? MapPosition { get; set; }
}
