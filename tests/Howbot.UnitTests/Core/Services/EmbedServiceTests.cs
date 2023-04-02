using System;
using Discord;
using Docker.DotNet;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Victoria.Player;
using Xunit;

namespace Howbot.UnitTests.Core.Services;

public class EmbedServiceTests
{
  private static IEmbedService /*, Mock<IServiceLocator>*/ Factory()
  {
    var serviceLocator = new Mock<IServiceLocator>();
    var logger = new Mock<ILoggerAdapter<EmbedService>>();

    _ = SetupCreateScope(serviceLocator);

    var embedService = new EmbedService(logger.Object);

    return embedService;
  }

  private static Tuple<Mock<IEmbedService>> SetupCreateScope(Mock<IServiceLocator> serviceLocator)
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

  private static Tuple<Mock<IEmbedService>> SetupCustomInjection(Mock<IServiceProvider> serviceProvider)
  {
    // GetRequiredService is an extension method, but GetService is not
    var dockerClient = new DockerClientConfiguration().CreateClient();

    serviceProvider
      .Setup(x => x.GetService(typeof(IEmbedService)))
      .Returns(dockerClient);

    return new Tuple<Mock<IEmbedService>>(new Mock<IEmbedService>());
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockEmbedOptions = new Mock<EmbedOptions>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service => service.CreateEmbed(mockEmbedOptions.Object))
      .Returns(new EmbedBuilder().Build());

    var result = embedService.CreateEmbed(new EmbedOptions());

    Assert.NotNull(result);
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_NowPlaying_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockLavaTrack = new Mock<LavaTrack>();
    var mockGuildUser = new Mock<IGuildUser>();
    var mockTextChannel = new Mock<ITextChannel>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service =>
        service.GenerateMusicNowPlayingEmbedAsync(mockLavaTrack.Object, mockGuildUser.Object, mockTextChannel.Object)
          .Result)
      .Returns(new EmbedBuilder().Build());

    var result =
      embedService.GenerateMusicNowPlayingEmbedAsync(mockLavaTrack.Object, mockGuildUser.Object,
        mockTextChannel.Object);

    Assert.NotNull(result);
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_NextTrack_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockQueue = new Mock<Vueue<LavaTrack>>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service => service.GenerateMusicNextTrackEmbedAsync(mockQueue.Object).Result)
      .Returns(new EmbedBuilder().Build());

    var result = embedService.GenerateMusicNextTrackEmbedAsync(mockQueue.Object);

    Assert.NotNull(result);
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_CurrentQueue_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockQueue = new Mock<Vueue<LavaTrack>>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service => service.GenerateMusicCurrentQueueEmbedAsync(mockQueue.Object).Result)
      .Returns(new EmbedBuilder().Build());

    var result = embedService.GenerateMusicCurrentQueueEmbedAsync(mockQueue.Object);

    Assert.NotNull(result);
  }
}
