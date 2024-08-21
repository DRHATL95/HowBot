using Howbot.Core.Interfaces;

namespace Howbot.Infrastructure.Messaging;

public class InMemoryQueueSender : IQueueSender
{
  public async Task SendMessageToQueueAsync(string? message, string queueName)
  {
    await Task.Run(() =>
    {
      lock (InMemoryQueueReceiver.MessageQueue)
      {
        InMemoryQueueReceiver.MessageQueue.Enqueue(message);
      }
    });
  }
}
