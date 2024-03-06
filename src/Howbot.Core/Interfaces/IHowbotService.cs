using System.Threading;
using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IHowbotService
{
  void Initialize();

  Task StartWorkerServiceAsync(CancellationToken cancellationToken);

  Task StopWorkerServiceAsync(CancellationToken cancellationToken);
}
