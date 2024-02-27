using System;
using System.Collections.Generic;

namespace Howbot.Core.Models.Commands;

public class CommandRequest
{
  public CommandTypes CommandType { get; init; }
  public IReadOnlyDictionary<string, string> Arguments { get; init; }
  public ulong GuildId { get; init; }
  public ulong ChannelId { get; init; }
  public CommandRequestMetadata Metadata { get; init; }
  
  public static CommandRequest Create(CreateCommandRequestParameters parameters)
  {
    return new CommandRequest
    {
      CommandType = parameters.CommandType,
      Arguments = parameters.Arguments,
      GuildId = parameters.GuildId,
      ChannelId = parameters.ChannelId,
      Metadata = parameters.Metadata
    };
  }
}

public struct CommandRequestMetadata
{
  public CommandSource Source { get; set; }
  
  public ulong RequestedById { get; set; }
  
  public DateTime RequestDateTime { get; set; }
}

public struct CreateCommandRequestParameters
{
  public CommandTypes CommandType { get; set; }
  public IReadOnlyDictionary<string, string> Arguments { get; set; }
  public ulong GuildId { get; set; }
  public ulong ChannelId { get; set; }
  public CommandRequestMetadata Metadata { get; set; }
}
