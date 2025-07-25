using Microsoft.AspNetCore.SignalR;

namespace Howbot.Web.Hubs;

public class BotHub(ILogger<BotHub> logger) : Hub
{
  public async Task JoinGuildGroupAsync(string guildId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"guild_{guildId}");
    logger.LogDebug("Connection {ConnectionId} joined guild group {GuildId}",
            Context.ConnectionId, guildId);
  }

  public async Task LeaveGuildGroup(string guildId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"guild_{guildId}");
    logger.LogDebug("Connection {ConnectionId} left guild group {GuildId}",
        Context.ConnectionId, guildId);
  }

  public override async Task OnConnectedAsync()
  {
    logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
    await base.OnDisconnectedAsync(exception);
  }

  // Hub methods called by the bot service
  public async Task NotifyMusicStatusChanged(string guildId, object status)
  {
    await Clients.Group($"guild_{guildId}").SendAsync("MusicStatusChanged", status);
  }

  public async Task NotifyQueueUpdated(string guildId, object queue)
  {
    await Clients.Group($"guild_{guildId}").SendAsync("QueueUpdated", queue);
  }

  public async Task NotifyPlayerConnected(string guildId, string channelName)
  {
    await Clients.Group($"guild_{guildId}").SendAsync("PlayerConnected", channelName);
  }

  public async Task NotifyPlayerDisconnected(string guildId)
  {
    await Clients.Group($"guild_{guildId}").SendAsync("PlayerDisconnected");
  }

  public async Task NotifyError(string guildId, string error)
  {
    await Clients.Group($"guild_{guildId}").SendAsync("ErrorOccurred", error);
  }
}
