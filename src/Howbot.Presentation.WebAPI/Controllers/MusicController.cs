using Howbot.Infrastructure.Audio.Lavalink.Services;
using Howbot.Infrastructure.Data;
using Lavalink4NET.Rest;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Presentation.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MusicController (RestMusicService musicService, ILavalinkApiClient lavalinkApiClient) : Controller
{

  [HttpGet]
  public async Task<ActionResult<IEnumerable<string>>> GetMusicQueue(ulong guildId)
  {
    var sessionId = await musicService.GetSessionIdForGuildIdAsync(guildId);
    if (string.IsNullOrEmpty(sessionId))
    {
      return NotFound("No music session found for the specified guild.");
    }

    var player = await lavalinkApiClient.GetPlayerAsync(sessionId, guildId);
    if (player == null)
    {
      return NotFound("No player found for the specified guild.");
    }

    return Ok(Enumerable.Empty<string>()); // Replace with actual queue retrieval logic
  }
}
