using Howbot.Core.Models.Commands;
using Howbot.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]/{guildId}")]
[ApiController]
public class PlaybackController(MessageQueuePublisherService publisherService) : Controller
{
  [HttpPost("play")]
  public async Task<IActionResult> Play(ulong guildId, [FromBody] PlayRequest request)
  {
    if (guildId <= 0)
    {
      return BadRequest("Invalid guild id");
    }

    var commandRequest = new ApiCommandRequest();
    var commandRequestJson = JsonConvert.SerializeObject(commandRequest);

    var commandResultJson = await publisherService.CallAsync(commandRequestJson);
    if (!string.IsNullOrEmpty(commandResultJson))
    {
      return BadRequest("Unable to respond to request");
    }

    var commandResponse = JsonConvert.DeserializeObject<ApiCommandResponse>(commandResultJson);
    if (commandResponse == null)
    {
      return BadRequest("Response unable to be deserialized");
    }

    if (commandResponse.Value is not null)
    {
      return Ok($"Now playing {request.SearchRequestRaw}");
    }

    return BadRequest("Unable to play. Try again later");
  }

  [HttpPost("pause")]
  public IActionResult Pause(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("resume")]
  public IActionResult Resume(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("stop")]
  public IActionResult Stop(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("skip")]
  public IActionResult Skip(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("seek")]
  public IActionResult Seek(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("volume")]
  public IActionResult Volume(ulong guildId)
  {
    return Ok();
  }

  [HttpPost("join")]
  public async Task<IActionResult> Join(ulong guildId)
  {
    if (guildId <= 0)
    {
      return BadRequest("Invalid guild id");
    }

    var commandRequestParams = new CreateApiCommandRequestParameters
    {
      CommandType = CommandTypes.JoinVoiceChannel,
      GuildId = guildId,
      Arguments = new Dictionary<string, string> { { "channelId", "1083117434443153518" } }
    };

    var commandRequest = ApiCommandRequest.Create(commandRequestParams);
    var commandRequestJson = JsonConvert.SerializeObject(commandRequest);

    var commandResponseJson = await publisherService.CallAsync(commandRequestJson);
    var commandResponse = JsonConvert.DeserializeObject<ApiCommandResponse>(commandResponseJson);

    if (commandResponse?.IsSuccessful ?? false)
    {
      return Ok("Joined voice");
    }

    return BadRequest("Unable to join");
  }
}

public struct PlayRequest
{
  public string SearchRequestRaw { get; set; }

  public Uri TrackUri { get; set; }
}
