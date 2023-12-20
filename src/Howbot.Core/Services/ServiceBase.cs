using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>(ILoggerAdapter<T> logger)
{
  private static readonly string _serviceName = typeof(T).Name;
  protected ILoggerAdapter<T> Logger => logger;

  public virtual void Initialize()
  {
    Logger.LogDebug($"{_serviceName} is initializing...");
  }
}
