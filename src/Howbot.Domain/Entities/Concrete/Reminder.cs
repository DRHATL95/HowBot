using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Howbot.Domain.Entities.Abstract;

namespace Howbot.Domain.Entities.Concrete;

public class Reminder : BaseEntity
{
  public ulong GuildId { get; set; }

  public ulong UserId { get; set; }

  public ulong TextChannelId { get; set; }

  public string Message { get; set; } = string.Empty;

  public DateTime RemindAt { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public GuildUser GuildUser { get; set; } = default!;
}
