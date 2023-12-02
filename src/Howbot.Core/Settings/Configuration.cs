using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET;

namespace Howbot.Core.Settings;

public static class Configuration
{
  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string YouTube = "YoutubeToken";
  private const string Postgres = "PostgresConnectionString";
  private const string Lavalink = "DiscordLavalinkServerPassword";
  private const string LavalinkAddress = "DiscordLavalinkServerAddress";

  public static string DiscordToken => GetTokenByName(IsDebug() ? DiscordTokenDev : DiscordTokenProd);

  public static string YouTubeToken => GetTokenByName(YouTube);

  public static string PostgresConnectionString => GetTokenByName(Postgres);

  private static string LavaNodePassword => GetTokenByName(Lavalink);

  private static string LavaNodeAddress => GetTokenByName(LavalinkAddress);

  private static GatewayIntents GatewayIntents => GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;

  /// <summary>
  ///   DiscordSocketClient configuration
  /// </summary>
  public static DiscordSocketConfig DiscordSocketConfig =>
    new()
    {
      AlwaysDownloadUsers = !IsDebug(),
      GatewayIntents = GatewayIntents,
      LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    };

  /// <summary>
  ///   Lavalink4NET configuration
  /// </summary>
  public static AudioServiceOptions AudioServiceOptions => new()
  {
    Passphrase = LavaNodePassword, BaseAddress = LavalinkUrl, HttpClientName = Constants.BotName
  };

  /// <summary>
  ///   Discord Interactions configuration
  /// </summary>
  public static InteractionServiceConfig InteractionServiceConfig =>
    new() { LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error };

  public static Uri LavalinkUrl { get; } = new(LavaNodeAddress);

  /// <summary>
  ///   Determines if the application is running in debug mode
  /// </summary>
  /// <returns>A boolean representing if application was ran in debug mode</returns>
  public static bool IsDebug()
  {
#if DEBUG
    return true;
#else
      return false;
#endif
  }

  /// <summary>
  /// Can be used to retrieve either env. variable or secrets using secret manager
  /// </summary>
  /// <param name="tokenName"></param>
  /// <returns></returns>
  private static string GetTokenByName(string tokenName)
  {
    // Should only be hit if empty, should never be null
    ArgumentException.ThrowIfNullOrEmpty(tokenName);

    // First attempt to get token, using hosted process
    string token = Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Process);
    // Second attempt to get token, using user environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.User);
    // Third attempt to get token, using machine environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Machine);

    if (string.IsNullOrEmpty(token))
    {
      // 9/27/23 - Add support for secrets.json
      token = ConfigurationHelper.HostConfiguration[tokenName];
    }

    return token ?? string.Empty;
  }
}
