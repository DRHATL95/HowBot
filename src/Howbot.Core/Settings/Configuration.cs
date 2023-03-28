using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Victoria.Node;
using Victoria.WebSocket;

namespace Howbot.Core.Settings;

public class Configuration
{
  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string LavalinkPassword = "DiscordLavalinkServerPassword";
  private const string YouTube = "Youtube";

  public static string DiscordToken => GetDiscordToken() ?? string.Empty;

  public string YouTubeToken => GetYouTubeToken() ?? string.Empty;

  private static GatewayIntents GatewayIntents => GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;

  public DiscordSocketConfig DiscordSocketConfig =>
    new()
    {
      AlwaysDownloadUsers = true,
      GatewayIntents = GatewayIntents,
      LogLevel = LogSeverity.Debug,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    };

  private WebSocketConfiguration WebSocketConfiguration => new() { BufferSize = 1024 };

  // LavaNode/Lavalink config
  public NodeConfiguration NodeConfiguration =>
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
}
