using System.Collections.Generic;

namespace Howbot.Core.Models;

public struct CommandPayload
{
  public string CommandName { get; set; }
  
  public Dictionary<string, string> CommandParameters { get; set; }

  public ulong ChannelId { get; set; }
  
  public ulong GuildId { get; set; }
  
  public string Message { get; set; }
  
  public ulong UserId { get; set; }
  
  public string Username { get; set; }
}
