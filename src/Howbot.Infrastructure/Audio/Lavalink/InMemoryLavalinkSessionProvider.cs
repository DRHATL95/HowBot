using System.Collections.Concurrent;
using Howbot.Infrastructure.Data;
using Howbot.SharedKernel;
using Lavalink4NET.Players;
using Lavalink4NET.Rest;

namespace Howbot.Infrastructure.Audio.Lavalink;
public class InMemoryLavalinkSessionProvider(ILavalinkApiClient lavalinkApiClient, AppDbContext db, ILoggerAdapter<InMemoryLavalinkSessionProvider> logger) : ILavalinkSessionProvider
{
  private readonly ConcurrentDictionary<ulong, LavalinkPlayerSession> _sessions = new();

  public Task<LavalinkPlayerSession?> GetSessionAsync(ulong guildId, CancellationToken cancellationToken = default)
  {
    _sessions.TryGetValue(guildId, out var session);
    return Task.FromResult<LavalinkPlayerSession?>(session);
  }

  public Task SetSessionAsync(ulong guildId, CancellationToken cancellationToken = default)
  {
    var guild = db.Guilds.FirstOrDefault(g => g.GuildId == guildId);
    if (guild is null)
    {
      return Task.CompletedTask;
    }

    var sessionId = guild.LavalinkSessionId;
    if (string.IsNullOrEmpty(sessionId))
    {
      return Task.CompletedTask;
    }

    _sessions[guildId] = new LavalinkPlayerSession
    {
      ApiClient = lavalinkApiClient,
      SessionId = sessionId,
      Label = "howbot-api-client"
    };

    logger.LogDebug("Successfully set Lavalink session for guild {GuildId} with session ID {SessionId}", guildId, sessionId);

    return Task.CompletedTask;
  }

  public Task RemoveSessionAsync(ulong guildId)
  {
    _sessions.TryRemove(guildId, out _);
    return Task.CompletedTask;
  }
}
