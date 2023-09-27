using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using Lavalink4NET;

namespace Howbot.Core.Settings;

public class Configuration
{
  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string YouTube = "YoutubeToken";
  private const string Postgres = "PostgresConnectionString";
  private const string Lavalink = "DiscordLavalinkServerPassword";

  [NotNull]
  public static string DiscordToken => GetTokenByName(IsDebug() ? DiscordTokenDev : DiscordTokenProd);

  [NotNull]
  public static string YouTubeToken => GetTokenByName(YouTube);

  public static string PostgresConnectionString => GetTokenByName(Postgres);

  private static string LavaNodePassword => GetTokenByName(Lavalink);

  private static GatewayIntents GatewayIntents => GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;

  public static DiscordSocketConfig DiscordSocketConfig =>
    new()
    {
      AlwaysDownloadUsers = true,
      GatewayIntents = GatewayIntents,
      LogLevel = LogSeverity.Info,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    };

  // Lavalink4Net Configuration 
  public static AudioServiceOptions AudioServiceOptions
  {
    get
    {
      return new AudioServiceOptions { Passphrase = LavaNodePassword };
    }
  }

  [NotNull]
  public static InteractionServiceConfig InteractionServiceConfig =>
    new() { LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error };

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
  private static string GetTokenByName([NotNull] string tokenName)
  {
    // Should only be hit if empty, should never be null
    ArgumentException.ThrowIfNullOrEmpty(tokenName);

    // First attempt to get token, using hosted process
    string token = Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Process);
    // Second attempt to get token, using user environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.User);
    // Third attempt to get token, using machine environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Machine);

    // 9/27/23 - Add support for secrets.json
    token = Helpers.ConfigurationHelper.HostConfiguration[tokenName];

    return token ?? string.Empty;
  }
}
