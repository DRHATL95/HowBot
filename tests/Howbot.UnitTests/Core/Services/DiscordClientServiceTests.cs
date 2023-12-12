using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Moq;
using Xunit;

namespace Howbot.UnitTests.Core.Services;

// TODO: AAA pattern - Arrange, Act, Assert

public class DiscordClientServiceTests
{
  private static (DiscordClientService, Mock<DiscordSocketClient>, Mock<InteractionService>, Mock<IServiceProvider>,
    Mock<ILoggerAdapter<DiscordClientService>>) Factory()
  {
    var discordSocketClient = new Mock<DiscordSocketClient>();
    var serviceProvider = new Mock<IServiceProvider>();
    var interactionService = new Mock<InteractionService>(discordSocketClient.Object, null!);
    var logger = new Mock<ILoggerAdapter<DiscordClientService>>();

    var discordClientService = new DiscordClientService(discordSocketClient.Object, serviceProvider.Object,
      interactionService.Object, logger.Object);

    return (discordClientService, discordSocketClient, interactionService, serviceProvider, logger);
  }

  [Fact]
  public async void LoginDiscordBot_WithValidToken()
  {
    // Arrange
    const string discordToken = "TestToken";
    var (discordClientService, _, _, _, _) = Factory();

    // TODO: Not sure if this is correct, because DiscordSocketClient is not able to Moq the LoginAsync method
    var mockedDiscordClientService = new Mock<IDiscordClientService>();
    mockedDiscordClientService
      .Setup(d => d.LoginDiscordBotAsync(discordToken))
      .Returns(Task.CompletedTask);

    // Act
    await discordClientService.LoginDiscordBotAsync(discordToken);
  }

  [Fact]
  public async void LoginDiscordBotAsync_ThrowsException()
  {
    // Arrange
    var (discordClientService, _, _, _, _) = Factory();

    var mockedDiscordClientService = new Mock<IDiscordClientService>();
    mockedDiscordClientService
      .Setup(d => d.LoginDiscordBotAsync(null))
      .ThrowsAsync(new ArgumentNullException());

    // Act
    var caughtException =
      await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        await discordClientService.LoginDiscordBotAsync(null));

    // Assert
    Assert.NotNull(caughtException);
  }
}
