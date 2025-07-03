using Howbot.Application.Models.Tarkov;

namespace Howbot.Application.Interfaces.Tarkov;

public interface IMapSwitchTarget
{
  public string Id { get; set; }

  public string? Name { get; set; }

  public MapPosition? MapPosition { get; set; }
}
