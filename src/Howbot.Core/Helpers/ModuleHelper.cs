using System;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Helpers;

public static class ModuleHelper
{
  public static void HandleCommandFailed<T>(CommandResponse commandResponse, ILoggerAdapter<T> logger)
  {
    ArgumentNullException.ThrowIfNull(commandResponse, nameof(commandResponse));
    ArgumentNullException.ThrowIfNull(logger, nameof(logger));

    logger.LogCommandFailed(commandResponse.CommandName);

    if (commandResponse.Exception != null)
    {
      throw commandResponse.Exception;
    }

    if (!string.IsNullOrEmpty(commandResponse.Message))
    {
      logger.LogError(commandResponse.Message);
    }
  }
}
