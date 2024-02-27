using System.Text;
using Discord.Rest;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Howbot.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommandController(MessageQueuePublisherService publisherService, DiscordRestClient discordRestClient) : Controller
{
  
  [HttpPost]
  public async Task<IActionResult> SendCommand([FromBody] CommandRequest commandRequest)
  {
    var message = Encoding.UTF8.GetBytes(commandRequest.ToString() ?? string.Empty);
    
    var response = await publisherService.CallAsync("message");

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
