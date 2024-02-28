using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Models.Commands;

namespace Howbot.Core.Interfaces;

public interface IHowbotService
{
  void Initialize();
  Task StartWorkerServiceAsync(CancellationToken cancellationToken);
  Task StopWorkerServiceAsync(CancellationToken cancellationToken);
  Task<CommandResponse> HandleCommandAsync(string commandAsJson, CancellationToken cancellationToken = default);
}
