using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public class DeploymentService : ServiceBase<DeploymentService>, IDeploymentService
{
  private readonly ILoggerAdapter<DeploymentService> _logger;

  public DeploymentService(ILoggerAdapter<DeploymentService> logger) : base(logger)
  {
    _logger = logger;
  }
}
