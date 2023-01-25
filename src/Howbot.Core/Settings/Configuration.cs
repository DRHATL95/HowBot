using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Victoria.Node;

namespace Howbot.Core.Settings;

public class Configuration
{
  private const string DiscordTokenDev = "DiscordTokenDev";
  private const string DiscordTokenProd = "DiscordTokenProd";
  private const string LavalinkPassword = "DiscordLavalinkServerPassword";
  
  public static string DiscordToken
  {
    get
    {
      return GetDiscordToken() ?? string.Empty;
    }
  }

  private static GatewayIntents GatewayIntents
  {
    get
    {
      return GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;
    }
  }

  public DiscordSocketConfig DiscordSocketConfig
  {
    get
    {
      return new DiscordSocketConfig()
      {
        AlwaysDownloadUsers = true,
        GatewayIntents = GatewayIntents,
        LogLevel = LogSeverity.Debug,
        LogGatewayIntentWarnings = false,
        UseInteractionSnowflakeDate = false,
      };
    }
  }
  
  // LavaNode/Lavalink config
  public NodeConfiguration NodeConfiguration
  {
    get
    {
      return new NodeConfiguration
      {
        Port = 2333, // TODO: dhoward - Move to web.config or .env
        Hostname = "localhost", // TODO: dhoward - Move to web.config or .env
        Authorization = GetLavaLinkPassword(),
        SelfDeaf = true,
        EnableResume = true,
        // SocketConfiguration = this.WebSocketConfiguration
      };
    }
  }

  public InteractionServiceConfig InteractionServiceConfig
  {
    get =>
      new()
      {
        LogLevel = IsDebug() ? LogSeverity.Debug : LogSeverity.Error
      };
  }

  public static bool IsDebug()
  {
    #if DEBUG
      return true;
    #else
      return false;
    #endif
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
    string token = null;

    if (IsDebug())
    {
      // See GetDiscordToken
      token = Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.Process);
      
      if (string.IsNullOrEmpty(token))
      {
        token = Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.User) ??
                Environment.GetEnvironmentVariable(LavalinkPassword, EnvironmentVariableTarget.Machine);
        
        return token ?? string.Empty;
      }
    }

    return token;
  }
}
