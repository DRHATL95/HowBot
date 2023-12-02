using System;
using Docker.DotNet;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Howbot.UnitTests.Core.Services;

public class DockerServiceTests
{
  private static IDockerService /*, Mock<IServiceLocator>*/ Factory()
  {
    var serviceLocator = new Mock<IServiceLocator>();

    _ = SetupCreateScope(serviceLocator);

    var logger = new Mock<ILoggerAdapter<DockerService>>();

    var dockerService = new DockerService(logger.Object);

    return dockerService;
  }

  private static Tuple<Mock<IDockerClient>> SetupCreateScope(Mock<IServiceLocator> serviceLocator)
  {
    var fakeScope = new Mock<IServiceScope>();
    serviceLocator
      .Setup(locator => locator.CreateScope())
      .Returns(fakeScope.Object);

    var serviceProvider = new Mock<IServiceProvider>();
    fakeScope
      .Setup(scope => scope.ServiceProvider)
      .Returns(serviceProvider.Object);

    return SetupCustomInjection(serviceProvider);
  }

  private static Tuple<Mock<IDockerClient>> SetupCustomInjection(Mock<IServiceProvider> serviceProvider)
  {
    // GetRequiredService is an extension method, but GetService is not
    var dockerClient = new DockerClientConfiguration().CreateClient();

    serviceProvider
      .Setup(x => x.GetService(typeof(IDockerClient)))
      .Returns(dockerClient);

    return new Tuple<Mock<IDockerClient>>(new Mock<IDockerClient>());
  }

  /*[Fact]
  public async Task DockerServiceTests_ListAllContainers_ReturnsIListContainerListResponse()
  {
    var dockerService = Factory();

    var mockDockerService = new Mock<IDockerService>();
    mockDockerService
      .Setup(service => service.ListAllContainers().Result)
      .Returns(new List<ContainerListResponse>());

    var result = await dockerService.ListAllContainers();

    Assert.True(result.Count > 0);
  }

  [Fact]
  public async Task DockerServiceTests_ListAllImages_ReturnsIListImageListResponse()
  {
    var dockerService = Factory();

    var mockDockerService = new Mock<IDockerService>();
    mockDockerService
      .Setup(service => service.ListAllImages().Result)
      .Returns(new List<ImagesListResponse>());

    var result = await dockerService.ListAllImages();

    Assert.True(result.Count > 0);
  }

  [Fact]
  public async Task DockerServiceTests_GetImageByImageName()
  {
    var dockerService = Factory();

    var mockDockerService = new Mock<IDockerService>();
    mockDockerService
      .Setup(service => service.GetImageByImageName("fredboat/lavalink").Result)
      .Returns(new ImageInspectResponse());

    var result = await dockerService.GetImageByImageName("fredboat/lavalink");

    Assert.NotNull(result);
  }

  [Fact]
  public async Task DockerServiceTests_BuildImageByImageName()
  {
    var dockerService = Factory();

    var mockDockerService = new Mock<IDockerService>();
    mockDockerService
      .Setup(service => service.BuildImageByImageName("fredboat/lavalink").Result)
      .Returns(true);

    var success = await dockerService.BuildImageByImageName("fredboat/lavalink");

    Assert.True(success);
  }

  [Fact]
  public async Task DockerServiceTests_BuildImageByImageNameAndTagName()
  {
    var dockerService = Factory();

    var mockDockerService = new Mock<IDockerService>();
    mockDockerService
      .Setup(service => service.BuildImageByImageName("fredboat/lavalink", "latest").Result)
      .Returns(true);

    var result = await dockerService.BuildImageByImageName("fredboat/lavalink", "latest");

    Assert.True(result);
  }*/
}
