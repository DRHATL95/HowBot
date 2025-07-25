using Howbot.Core.Models;

namespace Howbot.Web.Services;

public interface IBotApiService
{
  Task<bool> PlayMusicAsync(ulong guildId, string query, ulong userId);
  Task<bool> PauseMusicAsync(ulong guildId);
  Task<bool> ResumeMusicAsync(ulong guildId);
  Task<bool> StopMusicAsync(ulong guildId);
  Task<bool> SkipTrackAsync(ulong guildId);
  Task<MusicQueue?> GetQueueAsync(ulong guildId);
  Task<MusicStatus?> GetMusicStatusAsync(ulong guildId);
}

public class BotApiService(HttpClient httpClient, ILogger<BotApiService> logger) : IBotApiService
{
  public async Task<bool> PlayMusicAsync(ulong guildId, string query, ulong userId)
  {
    try
    {
      var request = new { GuildId = guildId, Query = query, UserId = userId };
      var response = await httpClient.PostAsJsonAsync("/api/music/play", request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send play command for guild {GuildId}", guildId);
      return false;
    }
  }

  public async Task<bool> PauseMusicAsync(ulong guildId)
  {
    try
    {
      var request = new { GuildId = guildId };
      var response = await httpClient.PostAsJsonAsync("/api/music/pause", request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send pause command for guild {GuildId}", guildId);
      return false;
    }
  }

  public async Task<bool> ResumeMusicAsync(ulong guildId)
  {
    try
    {
      var request = new { GuildId = guildId };
      var response = await httpClient.PostAsJsonAsync("/api/music/resume", request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send resume command for guild {GuildId}", guildId);
      return false;
    }
  }

  public async Task<bool> StopMusicAsync(ulong guildId)
  {
    try
    {
      var request = new { GuildId = guildId };
      var response = await httpClient.PostAsJsonAsync("/api/music/stop", request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send stop command for guild {GuildId}", guildId);
      return false;
    }
  }

  public async Task<bool> SkipTrackAsync(ulong guildId)
  {
    try
    {
      var request = new { GuildId = guildId };
      var response = await httpClient.PostAsJsonAsync("/api/music/skip", request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send skip command for guild {GuildId}", guildId);
      return false;
    }
  }

  public async Task<MusicQueue?> GetQueueAsync(ulong guildId)
  {
    try
    {
      var response = await httpClient.GetFromJsonAsync<MusicQueue>($"/api/music/{guildId}/queue");
      return response;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to get queue for guild {GuildId}", guildId);
      return null;
    }
  }

  public async Task<MusicStatus?> GetMusicStatusAsync(ulong guildId)
  {
    try
    {
      var response = await httpClient.GetFromJsonAsync<MusicStatus>($"/api/music/{guildId}/status");
      return response;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to get music status for guild {GuildId}", guildId);
      return null;
    }
  }
}
