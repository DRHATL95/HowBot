using System.ComponentModel.DataAnnotations;

namespace Howbot.Core.Entities;

public class Reminder : BaseEntity
{
  public ulong UserId { get; set; }

  public ulong ChannelId { get; set; }

  public string Message { get; set; } = string.Empty;

  public DateTime RemindAt { get; set; }

  public DateTime CreatedAt { get; set; }
}
