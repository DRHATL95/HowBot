using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IQueueSender
{
  Task SendMessageToQueue(string message, string queueName);
}
