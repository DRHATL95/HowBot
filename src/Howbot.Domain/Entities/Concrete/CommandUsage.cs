using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Howbot.Domain.Entities.Abstract;

namespace Howbot.Domain.Entities.Concrete;

public class CommandUsage : BaseEntity
{
  public ulong GuildUserId { get; set; }

  public ulong GuildId { get; set; }

  public string CommandName { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  public bool IsSuccess { get; set; } = false;

  // Relationships
  public GuildUser? GuildUser { get; set; }

  public Guild? Guild { get; set; }
}
