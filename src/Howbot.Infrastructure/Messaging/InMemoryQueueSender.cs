using System.Threading.Tasks;
using Howbot.Core.Interfaces;

namespace Howbot.Infrastructure.Messaging;

/// <summary>
///   Represents a class for sending messages to an in-memory queue.
/// </summary>
public class InMemoryQueueSender : IQueueSender
{
  /// <summary>
  ///   Send a message to a queue asynchronously.
  /// </summary>
  /// <param name="message">The message to be sent to the queue.</param>
  /// <param name="queueName">The name of the queue to which the message will be sent.</param>
  /// <returns>A Task representing the asynchronous operation.</returns>
  public async Task SendMessageToQueueAsync(string message, string queueName)
  {
    await Task.CompletedTask;

    InMemoryQueueReceiver.MessageQueue.Enqueue(message);

    /*await Task.Run(() =>
    {
      lock (InMemoryQueueReceiver.MessageQueue)
      {
        InMemoryQueueReceiver.MessageQueue.Enqueue(message);
      }
    });*/
  }
}
