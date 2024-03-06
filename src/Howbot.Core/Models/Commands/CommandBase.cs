using System;

namespace Howbot.Core.Models.Commands;
public abstract class CommandRequestBase
{
  public CommandTypes CommandType { get; init; }
  
  public ulong GuildId { get; set; }
  
  public CommandRequestMetadata Metadata { get; init; }
}

public struct CommandRequestMetadata
{
  public CommandSource Source { get; set; }

  public ulong RequestedById { get; set; }

  public DateTime RequestDateTime { get; set; }
}
