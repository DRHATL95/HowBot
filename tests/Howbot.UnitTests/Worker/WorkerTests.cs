using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Moq;
using Xunit;

namespace Howbot.UnitTests.Worker;

public class WorkerTests
{
  private readonly Mock<IHowbotService> _mockHowbotService;
  private readonly Mock<ILoggerAdapter<Howbot.Worker.Worker>> _mockLogger;
  private readonly Howbot.Worker.Worker _worker;

  public WorkerTests()
  {
    _mockHowbotService = new Mock<IHowbotService>();
    _mockLogger = new Mock<ILoggerAdapter<Howbot.Worker.Worker>>();
    _worker = new Howbot.Worker.Worker(_mockHowbotService.Object, _mockLogger.Object);
  }

  [Fact]
  public async Task StartAsync_ShouldStartWorkerService()
  {
    // Arrange
    var cancellationToken = new CancellationToken();

    // Act
    await _worker.StartAsync(cancellationToken);

    // Assert
    _mockHowbotService.Verify(service => service.StartWorkerServiceAsync(cancellationToken), Times.Once);
    _mockLogger.Verify(logger => logger.LogDebug("Starting the worker service"), Times.Once);
  }

  [Fact]
  public async Task StopAsync_ShouldStopWorkerService()
  {
    // Arrange
    var cancellationToken = new CancellationToken();

    // Act
    await _worker.StopAsync(cancellationToken);

    // Assert
    _mockHowbotService.Verify(service => service.StopWorkerServiceAsync(cancellationToken), Times.Once);
  }
}
