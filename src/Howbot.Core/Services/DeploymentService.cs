using System;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public class DeploymentService : IDeploymentService
{
  private readonly ILoggerAdapter<DeploymentService> _logger;

  public DeploymentService(ILoggerAdapter<DeploymentService> logger)
  {
    _logger = logger;
  }
  
  public void Initialize()
  {
    _logger.LogDebug("{ServiceName} is being initialized", nameof(DeploymentService));
  }
}
