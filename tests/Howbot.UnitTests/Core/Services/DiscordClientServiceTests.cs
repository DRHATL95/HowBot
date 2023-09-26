using System;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Howbot.UnitTests.Core.Services;

public class DiscordClientServiceTest
{
  private static (IDiscordClientService, Mock<DiscordSocketClient>, Mock<IAudioService>, Mock<ILoggerAdapter<DiscordClientService>>,
    Mock<IServiceProvider>, Mock<InteractionService>) Factory()
  {
    var serviceLocator = new Mock<IServiceLocator>();
    var discordSocketClient = new Mock<DiscordSocketClient>();
    var serviceProvider = new Mock<IServiceProvider>();
    var logger = new Mock<ILoggerAdapter<DiscordClientService>>();
    var audioService = new Mock<IAudioService>();
    // Services
    var interactionService = new Mock<InteractionService>();
    var voiceService = new Mock<VoiceService>();

    _ = SetupCreateScope(serviceLocator);

    var discordClientService = new DiscordClientService(discordSocketClient.Object, serviceProvider.Object, interactionService.Object, voiceService.Object, audioService.Object);

    return (discordClientService, discordSocketClient, audioService, logger, serviceProvider, interactionService);
  }

  private static Mock<DiscordSocketClient> SetupCreateScope(Mock<IServiceLocator> serviceLocator)
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

  private static Mock<DiscordSocketClient> SetupCustomInjection(Mock<IServiceProvider> serviceProvider)
  {
    // GetRequiredService is an extension method, but GetService is not
    var discordSocketClient = new Mock<DiscordSocketClient>();
    serviceProvider
      .Setup(x => x.GetService(typeof(DiscordSocketClient)))
      .Returns(discordSocketClient);

    return new Mock<DiscordSocketClient>(discordSocketClient);
  }

  /*[Fact]
  public void Initialize_WhenDiscordSocketClientIsNull_ShouldNotThrowException()
  {
    var (discordClientService, _, _, _, _, _, _) = Factory();
    discordClientService.Initialize();
  }*/
}
