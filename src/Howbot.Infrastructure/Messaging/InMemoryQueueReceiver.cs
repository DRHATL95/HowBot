using Ardalis.GuardClauses;
using Howbot.Core.Interfaces;

namespace Howbot.Infrastructure.Messaging;

public class InMemoryQueueReceiver : IQueueReceiver
{
  public static readonly Queue<string?> MessageQueue = new();
  
  public async Task<string?> GetMessageFromQueueAsync(string queueName)
  {
    Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

    return await Task.Run(() =>
    {
      lock (MessageQueue)
      {
        return MessageQueue.Count > 0 ? MessageQueue.Dequeue() : null;
      }
    });
  }
}
