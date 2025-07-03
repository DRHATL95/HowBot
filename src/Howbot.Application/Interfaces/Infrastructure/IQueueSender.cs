namespace Howbot.Application.Interfaces.Infrastructure;

public interface IQueueSender
{
  Task SendMessageToQueueAsync(string? message, string queueName);
}
