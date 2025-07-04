using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Howbot.Domain.Entities.Abstract;

namespace Howbot.Domain.Entities.Concrete;

public class CommandUsage : BaseEntity
{
  public ulong GuildId { get; set; }

  public ulong UserId { get; set; }

  public string CommandName { get; set; } = string.Empty;

  public bool IsSuccess { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public GuildUser GuildUser { get; set; } = default!;
}
