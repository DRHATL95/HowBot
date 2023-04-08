using System;
using System.IO;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Victoria.Node;
using Victoria.WebSocket;

namespace Howbot.Core.Settings;

public class Configuration
{
  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string LavalinkPassword = "DiscordLavalinkServerPassword";
  private const string YouTube = "YoutubeToken";

  public static string DiscordToken => GetDiscordToken() ?? string.Empty;

  public static string YouTubeToken => GetYouTubeToken() ?? string.Empty;
  
  public static string PostgresConnectionString => GetPostgresConnectionString() ?? string.Empty;
  
  public static IConfigurationRoot SerilogConfiguration => 
    new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      // .AddJsonFile($"config.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
      .AddJsonFile(path: "serilogconfig.json", optional: false, reloadOnChange: true)
      .Build();

  private static GatewayIntents GatewayIntents => GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;

  public static DiscordSocketConfig DiscordSocketConfig =>
    new()
    {
      AlwaysDownloadUsers = true,
      GatewayIntents = GatewayIntents,
      LogLevel = LogSeverity.Debug,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    };

  private static WebSocketConfiguration WebSocketConfiguration => new() { BufferSize = 2048 };

  // LavaNode/Lavalink config
  public static NodeConfiguration NodeConfiguration =>
    new()
    {
      Port = 2333, // TODO: dhoward - Move to web.config or .env
      Hostname = "localhost", // TODO: dhoward - Move to web.config or .env
      Authorization = GetLavaLinkPassword(),
      SelfDeaf = true,
      EnableResume = true,
      SocketConfiguration = WebSocketConfiguration
    };

  public InteractionServiceConfig InteractionServiceConfig =>
    new() { LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error };

  public static bool IsDebug()
  {
#if DEBUG
    return true;
#else
      return false;
#endif
  }

  private static string GetYouTubeToken()
  {
    string token = null;

    if (IsDebug())
    {
      // First attempt to get the token from the current hosted process.
      token = Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.Process);
      // ReSharper disable once InvertIf
      if (string.IsNullOrEmpty(token))
      {
        token = Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.Machine);

        return token ?? string.Empty;
      }
    }
    else
    {
      // First attempt to get the token from the current hosted process.
      token = Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.Process);
      // ReSharper disable once InvertIf
      if (string.IsNullOrEmpty(token))
      {
        token = Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(YouTube, EnvironmentVariableTarget.Machine);

        return token ?? string.Empty;
      }
    }

    return token;
  }

  private static string GetDiscordToken()
  {
    string token = null;

    if (IsDebug())
    {
      // First attempt to get the token from the current hosted process.
      token = Environment.GetEnvironmentVariable(DiscordTokenDev, EnvironmentVariableTarget.Process);
      // ReSharper disable once InvertIf
      if (string.IsNullOrEmpty(token))
      {
        token = Environment.GetEnvironmentVariable(DiscordTokenDev, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(DiscordTokenDev, EnvironmentVariableTarget.Machine);

        return token ?? string.Empty;
      }
    }
    else
    {
      // First attempt to get the token from the current hosted process.
      token = Environment.GetEnvironmentVariable(DiscordTokenProd, EnvironmentVariableTarget.Process);
      // ReSharper disable once InvertIf
      if (string.IsNullOrEmpty(token))
      {
        token = Environment.GetEnvironmentVariable(DiscordTokenProd, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(DiscordTokenProd, EnvironmentVariableTarget.Machine);

        return token ?? string.Empty;
      }
    }

    return token;
  }

  private static string GetLavaLinkPassword()
  {
    // See GetDiscordToken
    var token = Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.Process);

    if (!string.IsNullOrEmpty(token))
    {
      return token;
    }

    token = Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.Machine);

    return token ?? string.Empty;
  }
  
  private static string GetPostgresConnectionString()
  {
    // See GetDiscordToken
    var token = Environment.GetEnvironmentVariable("PostgresConnectionString", EnvironmentVariableTarget.Process);

    if (!string.IsNullOrEmpty(token))
    {
      return token;
    }

    token = Environment.GetEnvironmentVariable("PostgresConnectionString", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("PostgresConnectionString", EnvironmentVariableTarget.Machine);

    return token ?? string.Empty;
  }
}
