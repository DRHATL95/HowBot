using System.Text;
using Discord.Rest;
using Howbot.Core.Models;
using Howbot.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommandController(MessageQueuePublisherService publisherService, DiscordRestClient discordRestClient) : Controller
{
  
  [HttpPost]
  public async Task<IActionResult> SendCommand([FromBody] CommandPayload commandPayload)
  {
    var message = Encoding.UTF8.GetBytes(commandPayload.Message);
    
    var response = await publisherService.CallAsync("A message");

    return Ok(response);
  }
  
  [HttpGet]
  public async Task<IActionResult> GetCommands()
  {
    var commands = await discordRestClient.GetGlobalApplicationCommands();

    return commands == null
      ? NotFound()
      : Ok(commands);
  }
}
