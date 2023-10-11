using System;
using Discord;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Howbot.UnitTests.Core.Services;

public class EmbedServiceTests
{
  private static IEmbedService Factory()
  {
    var logger = new Mock<ILogger<EmbedService>>();

    var embedService = new EmbedService(logger.Object);

    return embedService;
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

    var lavalinkTrack = new Mock<LavalinkTrack>();
    var mockGuildUser = new Mock<IGuildUser>();
    var mockTextChannel = new Mock<ITextChannel>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service =>
        service.GenerateMusicNowPlayingEmbed(lavalinkTrack.Object, mockGuildUser.Object, mockTextChannel.Object,
          TimeSpan.FromMinutes(1), 100))
      .Returns(new EmbedBuilder().Build());

    var result =
      embedService.GenerateMusicNowPlayingEmbed(lavalinkTrack.Object, mockGuildUser.Object,
        mockTextChannel.Object, TimeSpan.FromMinutes(1), 100);

    Assert.NotNull(result);
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_NextTrack_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockQueue = new Mock<ITrackQueue>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service => service.GenerateMusicNextTrackEmbed(mockQueue.Object))
      .Returns(new EmbedBuilder().Build());

    var result = embedService.GenerateMusicNextTrackEmbed(mockQueue.Object);

    Assert.NotNull(result);
  }

  [Fact]
  public void EmbedServiceTests_CreateEmbed_CurrentQueue_ReturnsEmbed()
  {
    var embedService = Factory();

    var mockQueue = new Mock<ITrackQueue>();

    var mockEmbedService = new Mock<IEmbedService>();
    mockEmbedService
      .Setup(service => service.GenerateMusicCurrentQueueEmbed(mockQueue.Object))
      .Returns(new EmbedBuilder().Build());

    var result = embedService.GenerateMusicCurrentQueueEmbed(mockQueue.Object);

    Assert.NotNull(result);
  }
}
