using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public abstract class ServiceBase<T> : IServiceBase
{
  private readonly ILoggerAdapter<T> _logger;

  protected ServiceBase(ILoggerAdapter<T> logger)
  {
    _logger = logger;
  }

  public void Initialize()
  {
    if (_logger != null && _logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is initializing..", typeof(T).ToString());
    }
  }
}
