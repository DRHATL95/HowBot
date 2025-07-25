using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Events.Concrete;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure.Services;
public class HybridNotificationService : INotificationService, IAsyncDisposable
{
  private readonly ILoggerAdapter<HybridNotificationService> _logger;
  private readonly INotificationChannel _localChannel;
  private readonly HubConnection? _externalHubConnection;
  private bool _externalHubEnabled;

  // Event subscription for local consumers
  public event Func<ulong, MusicStatus, ValueTask>? MusicStatusChanged;
  public event Func<ulong, MusicQueue, ValueTask>? QueueUpdated;
  public event Func<ulong, string, ValueTask>? PlayerConnected;
  public event Func<ulong, Task>? PlayerDisconnected;
  public event Func<ulong, Exception, ValueTask>? ExceptionOccured;

  public HybridNotificationService(
      INotificationChannel localChannel,
      IConfiguration configuration,
      ILoggerAdapter<HybridNotificationService> logger)
  {
    _localChannel = localChannel;
    _logger = logger;

    // Try to connect to external SignalR hub if configured
    var externalHubUrl = configuration["ExternalSignalRHubUrl"];
    if (!string.IsNullOrEmpty(externalHubUrl))
    {
      try
      {
        _externalHubConnection = new HubConnectionBuilder()
            .WithUrl(externalHubUrl)
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10)])
            .Build();

        SetupExternalHubEventHandlers();
        _ = Task.Run(ConnectToExternalHubAsync);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to initialize external SignalR hub connection");
      }
    }
  }

  private void SetupExternalHubEventHandlers()
  {
    if (_externalHubConnection == null) return;

    _externalHubConnection.Reconnected += (connectionId) =>
    {
      _logger.LogInformation("External SignalR hub reconnected");
      _externalHubEnabled = true;
      return Task.CompletedTask;
    };

    _externalHubConnection.Closed += (error) =>
    {
      _logger.LogError("External SignalR hub connection closed");
      _externalHubEnabled = false;
      return Task.CompletedTask;
    };
  }

  private async ValueTask ConnectToExternalHubAsync()
  {
    if (_externalHubConnection == null) return;

    try
    {
      await _externalHubConnection.StartAsync();
      _externalHubEnabled = true;
      _logger.LogInformation("Connected to external SignalR hub");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to connect to external SignalR hub - continuing with local notifications only");
      _externalHubEnabled = false;
    }
  }

  public async ValueTask NotifyMusicStatusChangedAsync(ulong guildId, MusicStatus status)
  {
    // Local event
    if (MusicStatusChanged != null)
    {
      await MusicStatusChanged(guildId, status);
    }

    // Local channel
    await _localChannel.PublishAsync(NotificationChannels.MusicStatus,
        new NotificationMessage<MusicStatus> { GuildId = guildId, Data = status });

    // External SignalR hub (if available)
    if (_externalHubEnabled && _externalHubConnection != null)
    {
      try
      {
        await _externalHubConnection.InvokeAsync("NotifyMusicStatusChanged", guildId.ToString(), status);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send music status to external hub for guild {GuildId}", guildId);
      }
    }

    _logger.LogDebug("Music status notification sent for guild {GuildId}", guildId);
  }

  public async ValueTask NotifyMusicQueueChangedAsync(ulong guildId, MusicQueue queue)
  {
    // Local event
    if (QueueUpdated != null)
    {
      await QueueUpdated(guildId, queue);
    }

    // Local channel
    await _localChannel.PublishAsync(NotificationChannels.QueueUpdated,
        new NotificationMessage<MusicQueue> { GuildId = guildId, Data = queue });

    // External SignalR hub (if available)
    if (_externalHubEnabled && _externalHubConnection != null)
    {
      try
      {
        await _externalHubConnection.InvokeAsync("NotifyQueueUpdated", guildId.ToString(), queue);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send queue update to external hub for guild {GuildId}", guildId);
      }
    }

    _logger.LogDebug("Queue update notification sent for guild {GuildId}", guildId);
  }

  public async ValueTask NotifyMusicPlayerConnectedAsync(ulong guildId, string channelName)
  {
    // Local event
    if (PlayerConnected != null)
    {
      await PlayerConnected(guildId, channelName);
    }

    // Local channel
    await _localChannel.PublishAsync(NotificationChannels.PlayerConnection,
        new NotificationMessage<string> { GuildId = guildId, Data = channelName });

    // External SignalR hub (if available)
    if (_externalHubEnabled && _externalHubConnection != null)
    {
      try
      {
        await _externalHubConnection.InvokeAsync("NotifyPlayerConnected", guildId.ToString(), channelName);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send player connected to external hub for guild {GuildId}", guildId);
      }
    }

    _logger.LogDebug("Player connected notification sent for guild {GuildId}", guildId);
  }

  public async ValueTask NotifyMusicPlayerDisconnectedAsync(ulong guildId, string reason)
  {
    // Local event
    if (PlayerDisconnected != null)
    {
      await PlayerDisconnected(guildId);
    }

    // Local channel
    await _localChannel.PublishAsync(NotificationChannels.PlayerConnection,
        new NotificationMessage<string> { GuildId = guildId, Data = "Disconnected" });

    // External SignalR hub (if available)
    if (_externalHubEnabled && _externalHubConnection != null)
    {
      try
      {
        await _externalHubConnection.InvokeAsync("NotifyPlayerDisconnected", guildId.ToString());
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send player disconnected to external hub for guild {GuildId}", guildId);
      }
    }

    _logger.LogDebug("Player disconnected notification sent for guild {GuildId}", guildId);
  }

  public async ValueTask NotifyExceptionAsync(ulong guildId, Exception exception)
  {
    // Local event
    if (ExceptionOccured != null)
    {
      await ExceptionOccured(guildId, exception);
    }

    // Local channel
    await _localChannel.PublishAsync(NotificationChannels.Exceptions,
        new NotificationMessage<string> { GuildId = guildId, Data = exception.Message });

    // External SignalR hub (if available)
    if (_externalHubEnabled && _externalHubConnection != null)
    {
      try
      {
        await _externalHubConnection.InvokeAsync("NotifyError", guildId.ToString(), exception);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send error to external hub for guild {GuildId}", guildId);
      }
    }

    _logger.LogDebug("Error notification sent for guild {GuildId}", guildId);
  }

  public async ValueTask DisposeAsync()
  {
    if (_externalHubConnection != null)
    {
      await _externalHubConnection.DisposeAsync();
    }
  }
}
