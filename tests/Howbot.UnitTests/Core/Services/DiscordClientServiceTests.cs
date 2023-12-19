﻿using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Moq;
using Xunit;

namespace Howbot.UnitTests.Core.Services;

public class DiscordClientServiceTests
{
  private static (DiscordClientService, Mock<DiscordSocketClient>, Mock<IInteractionService>, Mock<IServiceProvider>,
    Mock<ILoggerAdapter<DiscordClientService>>) Factory()
  {
    var discordSocketClient = new Mock<DiscordSocketClient>();
    var serviceProvider = new Mock<IServiceProvider>();
    var interactionService = new Mock<IInteractionService>();
    var logger = new Mock<ILoggerAdapter<DiscordClientService>>();

    var discordClientService = new DiscordClientService(discordSocketClient.Object,
      interactionService.Object, logger.Object);

    return (discordClientService, discordSocketClient, interactionService, serviceProvider, logger);
  }

  [Fact]
  public async void LoginDiscordBotAsync_WithValidToken()
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

    // Assert
  }

  [Fact]
  public async void LoginDiscordBotAsync_ThrowsArgumentException()
  {
    // Arrange
    var token = string.Empty;
    var (discordClientService, _, _, _, _) = Factory();

    var mockedDiscordClientService = new Mock<IDiscordClientService>();
    mockedDiscordClientService
      .Setup(d => d.LoginDiscordBotAsync(token))
      .ThrowsAsync(new ArgumentException());

    // Act
    var caughtException =
      await Assert.ThrowsAsync<ArgumentException>(() =>
        discordClientService.LoginDiscordBotAsync(token));

    // Assert
    Assert.NotNull(caughtException);
  }

  [Fact]
  public async void StartDiscordBotAsync_ShouldStart()
  {
    // Arrange
    var (discordClientService, _, _, _, _) = Factory();
    var mockDiscordClientService = new Mock<IDiscordClientService>();

    // Act
    mockDiscordClientService
      .Setup(d => d.StartDiscordBotAsync())
      .Returns(Task.CompletedTask);

    await discordClientService.StartDiscordBotAsync();

    // Assert
  }
}
