using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Events.Abstract;

namespace Howbot.Core.Events.Concrete;
public class ExceptionOccuredEvent : BotNotificationEvent
{
  public Exception? Exception { get; set; }
}
