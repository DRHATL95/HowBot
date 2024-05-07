namespace Howbot.Core.Models.Commands;

public struct CommandMetadata
{
  public DateTime CommandDateTime { get; set; }

  public CommandSource Source { get; set; }
}
