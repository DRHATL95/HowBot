using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Howbot.Core.Interfaces;

namespace Howbot.Infrastructure.Messaging;

/// <summary>
/// Represents a receiver for an in-memory message queue.
/// </summary>
public class InMemoryQueueReceiver : IQueueReceiver
{
  /// <summary>
  /// Represents a message queue that stores messages as strings. </summary>
  /// /
  public static readonly Queue<string> MessageQueue = new();

  /// <summary>
  /// Retrieves a message from the message queue asynchronously.
  /// </summary>
  /// <returns>
  /// The retrieved message from the queue, or null if the queue is empty.
  /// </returns>
  public async Task<string> GetMessageFromQueueAsync(string queueName)
  {
    Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
    
    return await Task.Run(() =>
    {
      lock(MessageQueue)
      {
        return MessageQueue.Count > 0 ? MessageQueue.Dequeue() : null;
      }
    });
  }
}
