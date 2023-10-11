using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>
{
  private readonly string _serviceName = nameof(T);

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

  protected void HandleException(Exception exception, bool isFatal = false)
  {
    if (isFatal)
    {
      Logger.LogCritical(exception, "A fatal exception has been thrown in {ServiceName}.", _serviceName);
    }
    else
    {
      Logger.LogError(exception, "An exception has been thrown in {ServiceName}.", _serviceName);
    }
  }

  protected void HandleException(Exception exception, string callingFunctionName, bool isFatal = false)
  {
    if (isFatal)
    {
      Logger.LogCritical(exception, "A fatal exception has been thrown in {ServiceName} in {FunctionName}.",
        _serviceName, callingFunctionName);
    }
    else
    {
      Logger.LogError(exception, "An exception has been thrown in {ServiceName} in {FunctionName}.", _serviceName,
        callingFunctionName);
    }
  }
}
