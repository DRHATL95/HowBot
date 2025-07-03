namespace Howbot.Application.Interfaces.Infrastructure;

public interface IQueueReceiver
{
  Task<string?> GetMessageFromQueueAsync(string queueName);
}
