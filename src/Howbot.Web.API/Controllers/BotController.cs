using Discord;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Howbot.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BotController(MessageQueuePublisherService messageQueuePublisherService) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> Get()
  {
    var commandParams = new CreateApiCommandRequestParameters()
    {
      CommandType = CommandTypes.Guild, 
      GuildId = 656305202185633810, 
      UserId = 213133997520191489
    };
    
    ApiCommandRequest commandRequest = ApiCommandRequest.Create(commandParams);
    
    var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
    
    var commandResultJson = await messageQueuePublisherService.CallAsync(commandRequestJson);
    if (string.IsNullOrEmpty(commandResultJson))
    {
      return BadRequest("Unable to respond to request");
    }

    var settings = new JsonSerializerSettings();
    settings.Converters.Add(new ApiCommandResponseConverter());

    var commandResponse = JsonConvert.DeserializeObject<ApiCommandResponse>(commandResultJson, settings);
    if (commandResponse == null)
    {
      return BadRequest("Response unable to be deserialized");
    }

    if (commandResponse.IsSuccessful)
    {
      return Ok(commandResponse);
    }

    return BadRequest("Failed to send");
  }

  [HttpGet("guilds/{userId}")]
  public async Task<IActionResult> GetGuildsForUserId(ulong userId)
  {
    var commandParams = new CreateApiCommandRequestParameters()
    {
      CommandType = CommandTypes.Guilds, 
      UserId = userId
    };
    
    var commandRequest = ApiCommandRequest.Create(commandParams);
    var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
    
    var commandResponseJson = await messageQueuePublisherService.CallAsync(commandRequestJson);
    var commandResponse = JsonConvert.DeserializeObject<ApiCommandResponse>(commandResponseJson);

    if (commandResponse?.Value is null)
    {
      return BadRequest("Unable to get guilds for user");
    }
    
    // Parse the guilds from json string
    var guilds = JsonConvert.DeserializeObject<List<GuildDto>>(commandResponse.Value?.ToString() ?? string.Empty);
    
    return Ok(guilds);
  }
}
