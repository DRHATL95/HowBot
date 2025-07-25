using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Events.Abstract;
using Howbot.Core.Models;

namespace Howbot.Core.Events.Concrete;
public class QueueUpdateEvent : BotNotificationEvent
{
  public MusicQueue Queue { get; set; } = new();
}
