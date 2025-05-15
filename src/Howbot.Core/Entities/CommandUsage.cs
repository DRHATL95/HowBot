using System.ComponentModel.DataAnnotations;

namespace Howbot.Core.Entities;

public class CommandUsage : BaseEntity
{
  public ulong UserId { get; set; }

  public ulong GuildId { get; set; }

  public string CommandName { get; set; } = string.Empty;

  public DateTime Timestamp { get; set; }
}
