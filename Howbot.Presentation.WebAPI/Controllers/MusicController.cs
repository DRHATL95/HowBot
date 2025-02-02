using Lavalink4NET.Protocol.Models;
using Lavalink4NET.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Howbot.Presentation.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MusicController(
  ILavalinkApiClient lavalinkApiClient,
  IMemoryCache memoryCache,
  ILogger<MusicController> logger)
  : ControllerBase
{
  private async Task<PlayerInformationModel?> GetPlayerInformationAsync(string sessionId, ulong guildId)
  {
    if (memoryCache.TryGetValue("guild_player", out object? value) && value is PlayerInformationModel playerInformation)
    {
      return playerInformation;
    }

    var player = await lavalinkApiClient.GetPlayerAsync(sessionId, guildId);
    if (player is not null)
    {
      return player;
    }

    logger.LogWarning("Unable to find player for guild [{GuildId}]", guildId);
    return null;

  }

  [HttpGet("test")]
  public async Task<PlayerInformationModel?> TestingPlayerGet()
  {
    var player = await GetPlayerInformationAsync("123", 12345);

    return player;
  }

  [HttpPost("play")]
  public async Task<IActionResult> PlayAsync([FromQuery] string query, [FromQuery] ulong guildId)
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      return BadRequest("Query is required");
    }

    var sessionId = memoryCache.Get<string>("lavalink_session_id");
    if (string.IsNullOrWhiteSpace(sessionId))
    {
      return BadRequest("Session ID is required");
    }

    var player = await GetPlayerInformationAsync(sessionId, guildId);
    if (player is null)
    {
      return BadRequest("Player is required");
    }

    try
    {
      _ = await lavalinkApiClient.UpdatePlayerAsync(sessionId, guildId, new Lavalink4NET.Protocol.Requests.PlayerUpdateProperties
      {
        Identifier = query,
      });
    }
    catch (Exception)
    {
      logger.LogWarning("Unable to play track");
      return NoContent();
    }

    return Ok();
  }

  /*private async Task<string> GetSessionIdAsync()
  {
    if (_memoryCache.TryGetValue("lavalink_session_id", out string sessionId))
    {
      return sessionId;
    }

    var sessions = await _lavalinkApiClient.
  }

  [HttpGet("play")]
  public async Task<IActionResult> PlayAsync([FromQuery] string query)
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      return BadRequest("Query is required");
    }

    var player = await _lavalinkApiClient.GetPlayerAsync();

    var response = await _lavalinkApiClient.PlayAsync(query);
    return Ok(response);
  }

  [HttpGet("stop")]
  public async Task<IActionResult> StopAsync()
  {
    var response = await _lavalinkApiClient.StopAsync();
    return Ok(response);
  }

  [HttpGet("pause")]
  public async Task<IActionResult> PauseAsync()
  {
    var response = await _lavalinkApiClient.PauseAsync();
    return Ok(response);
  }

  [HttpGet("resume")]
  public async Task<IActionResult> ResumeAsync()
  {
    var response = await _lavalinkApiClient.ResumeAsync();
    return Ok(response);
  }

  [HttpGet("skip")]
  public async Task<IActionResult> SkipAsync()
  {
    var response = await _lavalinkApiClient.SkipAsync();
    return Ok(response);
  }

  [HttpGet("volume")]
  public async Task<IActionResult> VolumeAsync([FromQuery] int volume)
  {
    if (volume < 0 || volume > 100)
    {
      return BadRequest("Volume must be between 0 and 100");
    }
    var response = await _lavalinkApiClient.VolumeAsync(volume);
    return Ok(response);
  }*/
}
