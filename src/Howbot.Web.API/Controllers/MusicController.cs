using System.Collections.ObjectModel;
using Howbot.Core.Models.Commands;
using Howbot.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MusicController(MessageQueuePublisherService publisherService) : Controller
{
  [HttpGet("{guildId}")]
  public async Task<IActionResult> Index(ulong guildId, ulong channelId, ulong userId)
  {
    
    CreateCommandRequestParameters parameters = new CreateCommandRequestParameters
    {
      CommandType = CommandTypes.Queue,
      Arguments = ReadOnlyDictionary<string, string>.Empty,
      GuildId = guildId,
      ChannelId = channelId,
      Metadata = new CommandRequestMetadata
      {
        RequestDateTime = DateTime.Now, RequestedById = userId, Source = CommandSource.Api
      }
    };
    
    CommandRequest commandRequest = CommandRequest.Create(parameters);
    
    // Convert the request to a JSON string and publish it to the message queue
    var commandAsJson = JsonConvert.SerializeObject(commandRequest);
    
    var responseAsJson = await publisherService.CallAsync(commandAsJson);
    if (string.IsNullOrEmpty(responseAsJson))
    {
      return NotFound();
    }

    // Convert response from JSON to a CommandResponse object
    var response = JsonConvert.DeserializeObject<CommandResponse>(responseAsJson);
    if (response is { IsSuccessful: true })
    {
      return Ok();
    }

    return NotFound();
  }
}
