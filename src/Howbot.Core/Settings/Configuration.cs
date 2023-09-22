using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using Lavalink4NET;

namespace Howbot.Core.Settings;

public class Configuration
{

  #region Private Fields

  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string YouTube = "YoutubeToken";
  private const string Postgres = "PostgresConnectionString";
  private const string Lavalink = "DiscordLavalinkServerPassword";

  #endregion

  #region Public Fields

  [NotNull]
  public static string DiscordToken => GetEnvironmentalVariableByToken(IsDebug() ? DiscordTokenDev : DiscordTokenProd);

  [NotNull]
  public static string YouTubeToken => GetEnvironmentalVariableByToken(YouTube);

  public static string PostgresConnectionString => GetEnvironmentalVariableByToken(Postgres);

  private static string LavaNodePassword => GetEnvironmentalVariableByToken(Lavalink);

  private static GatewayIntents GatewayIntents => GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;

  public static DiscordSocketConfig DiscordSocketConfig =>
    new()
    {
      // AlwaysDownloadUsers = true,
      GatewayIntents = GatewayIntents,
      // LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error,
      LogLevel = LogSeverity.Info,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    };

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

  public static double MusixMatchVersionNumber => 1.1;

  #endregion

  public static bool IsDebug()
  {
    #if DEBUG
      return true;
    #else
      return false;
    #endif
  }

  private static string GetEnvironmentalVariableByToken([NotNull] string tokenName)
  {
    // Should only be hit if empty, should never be null
    ArgumentException.ThrowIfNullOrEmpty(tokenName);

    // First attempt to get token, using hosted process
    string token = Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Process);
    // Second attempt to get token, using user environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.User);
    // Third attempt to get token, using machine environment variables
    token ??= Environment.GetEnvironmentVariable(tokenName, EnvironmentVariableTarget.Machine);

    return token ?? string.Empty;
  }

}
