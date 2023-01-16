using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Victoria.Node;
using Victoria.WebSocket;

namespace Howbot.Core.Settings;

public class Configuration
{
  private readonly ILoggerAdapter<Configuration> _logger;
  private readonly TimeSpan _socketReconnectDelay = TimeSpan.FromSeconds(30);

  private const string DiscordTokenDev = "DiscordTokenDEV";
  private const string DiscordTokenProd = "DiscordTokenPROD";
  private const string LavalinkPassword = "LavalinkServerPassword";
  private const int SocketReconnectAttempts = 3;
  
  public string DiscordToken
  {
    get
    {
      return GetDiscordToken() ?? string.Empty;
    }
  }

  public GatewayIntents GatewayIntents
  {
    get
    {
      // TODO: dhoward - Update to appropriate privileges
      return GatewayIntents.AllUnprivileged;
    }
  }

  public DiscordSocketConfig DiscordSocketConfig
  {
    get
    {
      return new DiscordSocketConfig()
      {
        GatewayIntents = GatewayIntents, AlwaysDownloadUsers = false, LogLevel = LogSeverity.Debug, LogGatewayIntentWarnings = false
      };
    }
  }
  
  public NodeConfiguration NodeConfiguration
  {
    get =>
      new()
      {
        Port = 2333, // TODO: dhoward - Move to web.config or .env
        Hostname = "localhost", // TODO: dhoward - Move to web.config or .env
        Authorization = GetLavaLinkPassword(),
        SelfDeaf = true,
        EnableResume = true
        // SocketConfiguration = this.WebSocketConfiguration
      };
  }
  
  private WebSocketConfiguration WebSocketConfiguration
  {
    get =>
      new()
      {
        ReconnectAttempts = SocketReconnectAttempts, ReconnectDelay = _socketReconnectDelay // 30 seconds
      };
  }

  public InteractionServiceConfig InteractionServiceConfig
  {
    get =>
      new()
      {
        // AutoServiceScopes = true,
        // DefaultRunMode = RunMode.Async,
        
        LogLevel = this.IsDebug() ? LogSeverity.Debug : LogSeverity.Error
      };
  }

  public Configuration(ILoggerAdapter<Configuration> logger)
  {
    _logger = logger;
  }

  public bool IsDebug()
  {
    #if DEBUG
      return true;
    #else
      return false;
    #endif
  }

  private string GetDiscordToken()
  {
    string token = null;

    if (this.IsDebug())
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

  private string GetLavaLinkPassword()
  {
    string token = null;

    if (this.IsDebug())
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
