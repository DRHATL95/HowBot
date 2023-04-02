using System;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.UnitTests.Core.Services;

public class DiscordClientServiceTest
{
  private static (IDiscordClientService, Mock<DiscordSocketClient>, Mock<ILoggerAdapter<DiscordClientService>>,
    Mock<IServiceProvider>, Mock<InteractionService>, Mock<LavaNodeService>,
    Mock<LavaNode<Player<LavaTrack>, LavaTrack>>) Factory()
  {
    var serviceLocator = new Mock<IServiceLocator>();
    var discordSocketClient = new Mock<DiscordSocketClient>();
    var serviceProvider = new Mock<IServiceProvider>();
    var lavaNode = new Mock<LavaNode<Player<LavaTrack>, LavaTrack>>();
    var logger = new Mock<ILoggerAdapter<DiscordClientService>>();
    // Services
    var interactionService = new Mock<InteractionService>();
    var lavaNodeService = new Mock<LavaNodeService>();

    _ = SetupCreateScope(serviceLocator);

    var discordClientService = new DiscordClientService(discordSocketClient.Object, lavaNodeService.Object,
      serviceProvider.Object, interactionService.Object, lavaNode.Object, logger.Object);

    return (discordClientService, discordSocketClient, logger, serviceProvider, interactionService, lavaNodeService,
      lavaNode);
  }

  private static Tuple<Mock<DiscordSocketClient>, Mock<LavaNode<Player<LavaTrack>, LavaTrack>>, Mock<LavaNodeService>>
    SetupCreateScope(Mock<IServiceLocator> serviceLocator)
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

  private static Tuple<Mock<DiscordSocketClient>, Mock<LavaNode<Player<LavaTrack>, LavaTrack>>, Mock<LavaNodeService>>
    SetupCustomInjection(Mock<IServiceProvider> serviceProvider)
  {
    // GetRequiredService is an extension method, but GetService is not
    var discordSocketClient = new Mock<DiscordSocketClient>();
    serviceProvider
      .Setup(x => x.GetService(typeof(DiscordSocketClient)))
      .Returns(discordSocketClient);

    var lavaNode = new Mock<LavaNode<Player<LavaTrack>, LavaTrack>>();
    serviceProvider
      .Setup(x => x.GetService(typeof(LavaNode<Player<LavaTrack>, LavaTrack>)))
      .Returns(lavaNode);

    var lavaNodeService = new Mock<LavaNodeService>();
    serviceProvider
      .Setup(x => x.GetService(typeof(LavaNodeService)))
      .Returns(lavaNodeService);

    return new Tuple<Mock<DiscordSocketClient>, Mock<LavaNode<Player<LavaTrack>, LavaTrack>>, Mock<LavaNodeService>>(
      discordSocketClient, lavaNode, lavaNodeService);
  }

  /*[Fact]
  public void Initialize_WhenDiscordSocketClientIsNull_ShouldNotThrowException()
  {
    var (discordClientService, _, _, _, _, _, _) = Factory();
    discordClientService.Initialize();
  }*/
}
