using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>
{
  private readonly string _serviceName = nameof(T);

  protected ServiceBase()
  {
    Logger = new LoggerFactory().CreateLogger<T>();
  }

  protected ServiceBase([NotNull] ILogger<T> logger)
  {
    Logger = logger;
  }

  [NotNull] protected ILogger<T> Logger { get; }

  public virtual void Initialize()
  {
    if (Logger.IsEnabled(LogLevel.Debug))
    {
      Logger.LogDebug("{ServiceName} is initializing..", _serviceName);
    }
  }

  protected void HandleException(Exception exception)
  {
    if (Logger.IsEnabled(LogLevel.Error))
    {
      Logger.LogError(exception, "An exception has been thrown in {ServiceName}.", _serviceName);
    }
  }
}
