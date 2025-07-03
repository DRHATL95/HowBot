namespace Howbot.Application.Interfaces.Discord;

public interface IHowbotService
{
  void Initialize();

  Task StartWorkerServiceAsync(CancellationToken cancellationToken);

  Task StopWorkerServiceAsync(CancellationToken cancellationToken);
}
