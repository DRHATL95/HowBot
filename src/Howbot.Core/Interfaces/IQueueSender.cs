using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IQueueSender
{
  Task SendMessageToQueueAsync(string message, string queueName);
}
