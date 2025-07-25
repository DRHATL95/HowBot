using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Core.Events.Abstract;
public abstract class BotNotificationEvent()
{
  public ulong GuildId { get; }
  public string? Message { get; }
  public DateTime Timestamp { get; } = DateTime.UtcNow;
}
