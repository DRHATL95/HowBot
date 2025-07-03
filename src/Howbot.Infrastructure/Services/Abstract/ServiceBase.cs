using Howbot.SharedKernel;

namespace Howbot.Infrastructure.Services.Abstract;

public abstract class ServiceBase<T>(ILoggerAdapter<T> logger)
{
  private static readonly string _serviceName = typeof(T).Name;
  protected ILoggerAdapter<T> Logger => logger;

  public virtual void Initialize()
  {
    Logger.LogDebug($"{_serviceName} is initializing...");
  }

  public virtual Task InitializeAsync()
  {
    Logger.LogDebug($"{_serviceName} is initializing...");

    return Task.CompletedTask;
  }
}
