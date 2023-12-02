using System;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T>
{
  private const string ServiceName = nameof(ServiceBase<T>);

  protected ServiceBase(ILoggerAdapter<T> logger)
  {
    Logger = logger;
  }

  protected ILoggerAdapter<T> Logger { get; }

  /// <summary>
  ///   Every service needs to implement this method. This will hook up events primarily,
  ///   but will also log that the service has been initialized.
  /// </summary>
  public virtual void Initialize()
  {
    // Generically obtain the service name class and log that it is initializing.
    Logger.LogDebug("{ServiceName} is initializing..", ServiceName);
  }

  protected void HandleException(Exception exception, bool isFatal = false)
  {
    if (isFatal)
    {
      Logger.LogCritical(exception, "A fatal exception has been thrown in {ServiceName}.", ServiceName);
    }
    else
    {
      Logger.LogError(exception, "An exception has been thrown in {ServiceName}.", ServiceName);
    }
  }

  protected void HandleException(Exception exception, string callingFunctionName, bool isFatal = false)
  {
    if (isFatal)
    {
      Logger.LogCritical(exception, "A fatal exception has been thrown in {ServiceName} in {FunctionName}.",
        ServiceName, callingFunctionName);
    }
    else
    {
      Logger.LogError(exception, "An exception has been thrown in {ServiceName} in {FunctionName}.", ServiceName,
        callingFunctionName);
    }
  }
}
