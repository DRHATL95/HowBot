using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>(ILoggerAdapter<T> logger)
{
  private const string ServiceName = nameof(T);
  protected ILoggerAdapter<T> Logger => logger;

  public virtual void Initialize()
  {
    Logger.LogDebug($"{ServiceName} is initializing...");
  }
}
