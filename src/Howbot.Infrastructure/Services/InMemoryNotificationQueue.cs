using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Events.Abstract;
using Howbot.Core.Interfaces;

namespace Howbot.Infrastructure.Services;
public class InMemoryNotificationQueue(ILoggerAdapter<InMemoryNotificationQueue> logger)
{
  private readonly ConcurrentQueue<BotNotificationEvent> _botNotificationEvents = new();
  private readonly SemaphoreSlim _semaphore = new(1, 1);

  public int Count => _botNotificationEvents.Count;

  public async ValueTask EnqueueAsync(BotNotificationEvent notificationEvent)
  {
    await _semaphore.WaitAsync();
    try
    {
      _botNotificationEvents.Enqueue(notificationEvent);
      logger.LogInformation("Enqueued notification event: {EventType}", notificationEvent.GetType().Name);
    }
    finally
    {
      _semaphore.Release();
    }
  }

  public async ValueTask<BotNotificationEvent?> DequeueAsync()
  {
    await _semaphore.WaitAsync();
    try
    {
      if (_botNotificationEvents.TryDequeue(out var notificationEvent))
      {
        logger.LogInformation("Dequeued notification event: {EventType}", notificationEvent.GetType().Name);
        return notificationEvent;
      }
      logger.LogInformation("No notification events to dequeue");
      return null;
    }
    finally
    {
      _semaphore.Release();
    }
  }

  public async ValueTask<IEnumerable<BotNotificationEvent>> DequeueAllAsync()
  {
    await _semaphore.WaitAsync();
    try
    {
      var events = new List<BotNotificationEvent>();
      while (_botNotificationEvents.TryDequeue(out var notificationEvent))
      {
        events.Add(notificationEvent);
      }
      logger.LogInformation("Dequeued {Count} notification events", events.Count);
      return events;
    }
    finally
    {
      _semaphore.Release();
    }
  }
}
