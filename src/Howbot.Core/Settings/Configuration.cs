using System;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Lavalink4NET;
using RabbitMQ.Client;
using Constants = Howbot.Core.Models.Constants;

namespace Howbot.Core.Settings;

/// <summary>
///   The Configuration class provides access to various configuration settings used in the application.
/// </summary>
public static class Configuration
{
  // The names of the environment variable
  private const string DiscordApiToken = "DiscordToken";

  private const string DiscordOAuthClientIdKey = "DiscordOAuthClientId";
  private const string DiscordOAuthClientSecretKey = "DiscordOAuthClientSecret";

  // private const string YouTube = "YoutubeToken";
  private const string Postgres = "HowbotPostgres";

  private const string LavalinkPassword = "LavalinkNodePassword";
  private const string LavalinkAddress = "LavalinkNodeAddress";
  private const string WatchTogetherKey = "Watch2GetherKey";

  private const string RabbitMqHostNameKey = "RabbitMQHost";
  private const string RabbitMqPortKey = "RabbitMQPort";
  private const string RabbitMqUserNameKey = "RabbitMQUser";
  private const string RabbitMqPasswordKey = "RabbitMQPassword";

  private const string SpotifyClientIdKey = "SpotifyClientId";
  private const string SpotifyClientSecretKey = "SpotifyClientSecret";

  public static string DiscordToken => GetTokenByName(DiscordApiToken);

  public static string DiscordOAuthClientId => GetTokenByName(DiscordOAuthClientIdKey);

  public static string DiscordOAuthClientSecret => GetTokenByName(DiscordOAuthClientSecretKey);

  // public static string YouTubeToken => GetTokenByName(YouTube);

  public static string PostgresConnectionString => GetTokenByName(Postgres);

  public static string WatchTogetherApiKey => GetTokenByName(WatchTogetherKey);

  private static string LavaNodePassword => GetTokenByName(LavalinkPassword);

  private static string LavaNodeAddress => GetTokenByName(LavalinkAddress);

  private static string RabbitMqHostName => GetTokenByName(RabbitMqHostNameKey);

  private static string RabbitMqPort => GetTokenByName(RabbitMqPortKey);

  private static string RabbitMqUserName => GetTokenByName(RabbitMqUserNameKey);

  private static string RabbitMqPassword => GetTokenByName(RabbitMqPasswordKey);
  
  public static string SpotifyClientId => GetTokenByName(SpotifyClientIdKey);
  
  public static string SpotifyClientSecret => GetTokenByName(SpotifyClientSecretKey);

  /// <summary>
  ///   Represents the gateway intents to subscribe to.
  /// </summary>
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
    Passphrase = LavaNodePassword,
    BaseAddress = LavalinkUri,
    HttpClientName = Constants.Discord.BotName
  };

  /// <summary>
  ///   Discord Interactions configuration
  /// </summary>
  public static InteractionServiceConfig InteractionServiceConfig =>
    new() { LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error };

  public static Uri LavalinkUri { get; } = new(LavaNodeAddress);

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

  public static ConnectionFactory RabbitMqConnectionFactory => new()
  {
    HostName = RabbitMqHostName,
    Port = int.TryParse(RabbitMqPort, out var portAsInt) ? portAsInt : 5672,
    UserName = RabbitMqUserName,
    Password = RabbitMqPassword
  };

  /// <summary>
  ///   Can be used to retrieve either env. variable or secrets using secret manager
  /// </summary>
  /// <param name="tokenName"></param>
  /// <returns></returns>
  private static string GetTokenByName(string tokenName)
  {
    // Should only be hit if empty, should never be null
    Guard.Against.NullOrEmpty(tokenName, nameof(tokenName));

    var token = Environment.GetEnvironmentVariable(tokenName);

    // First attempt to get token, using hosted process
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Process);
    // Second attempt to get token, using user environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.User);
    // Third attempt to get token, using machine environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Machine);

    if (string.IsNullOrEmpty(token))
    {
      // 9/27/23 - Add support for secrets.json
      token = ConfigurationHelper.HostConfiguration?[tokenName];
    }

    return token ?? string.Empty;
  }
}
