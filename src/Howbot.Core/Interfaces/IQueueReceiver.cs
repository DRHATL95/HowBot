using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IQueueReceiver
{
  Task<string> GetMessageFromQueue(string queueName);
}
