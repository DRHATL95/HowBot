using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure.Services;
public class InMemoryNotificationChannel(ILogger<InMemoryNotificationChannel> logger) : INotificationChannel
{
  private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _subscribers = new();

  public ValueTask PublishAsync<T>(string channel, T message) where T : class
  {
    if (_subscribers.TryGetValue(channel, out var handlers))
    {
      var tasks = handlers.Select(handler =>
      {
        try
        {
          return handler(message);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error executing notification handler for channel {Channel}", channel);
          return Task.CompletedTask;
        }
      });

      _ = Task.Run(async () =>
      {
        await Task.WhenAll(tasks);
      });

      logger.LogDebug("Published message to channel {Channel} with {HandlerCount} handlers",
          channel, handlers.Count);
    }

    return ValueTask.CompletedTask;
  }

  public ValueTask SubscribeAsync<T>(string channel, Func<T, Task> handler) where T : class
  {
    _subscribers.AddOrUpdate(channel,
        [message => handler((T)message)],
        (key, existing) =>
        {
          existing.Add(message => handler((T)message));
          return existing;
        });

    logger.LogDebug("Subscribed to channel {Channel}", channel);
    return ValueTask.CompletedTask;
  }

  public ValueTask UnsubscribeAsync(string channel)
  {
    _subscribers.TryRemove(channel, out _);
    logger.LogDebug("Unsubscribed from channel {Channel}", channel);
    return ValueTask.CompletedTask;
  }
}
