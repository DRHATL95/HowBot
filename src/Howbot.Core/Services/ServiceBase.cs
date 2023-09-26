using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>
{
  [NotNull] 
  protected ILogger<T> Logger { get; }

  protected ServiceBase()
  {
    Logger = new LoggerFactory().CreateLogger<T>();
  }

  protected ServiceBase([NotNull] ILogger<T> logger)
  {
    Logger = logger;
  }
  
  public void Initialize()
  {
    if (Logger.IsEnabled(LogLevel.Debug))
    {
      Logger.LogDebug("{ServiceName} is initializing..", nameof(T));
    }
  }
}
