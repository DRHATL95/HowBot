using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;
public interface INotificationChannel
{
  ValueTask PublishAsync<T>(string channel, T message) where T : class;
  ValueTask SubscribeAsync<T>(string channel, Func<T, Task> handler) where T : class;
  ValueTask UnsubscribeAsync(string channel);
}
