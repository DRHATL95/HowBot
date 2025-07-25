using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Core.Events.Concrete;
public class NotificationMessage<T> where T : class
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public ulong GuildId { get; set; }
  public string EventType { get; set; } = typeof(T).Name;
  public T Data { get; set; } = default!;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class NotificationChannels
{
  public const string MusicStatus = "music:status";
  public const string QueueUpdated = "music:queue";
  public const string PlayerConnection = "player:connection";
  public const string Exceptions = "bot:exceptions";
}
