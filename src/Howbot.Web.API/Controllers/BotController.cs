using Discord.Rest;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BotController(DiscordRestClient discordRestClient) : Controller
{
  [HttpGet("commands")]
  public async Task<IActionResult> GetCommands()
  {
    var howbotCommands = await discordRestClient.GetGlobalApplicationCommands();
    
    var commands = howbotCommands.Select(command => new
    {
      command.Name,
      command.Description
    });
    
    return Ok(commands);
  }
  
  [HttpGet]
  public IActionResult Index()
  {
    return Ok("Howbot API is running");
  }
}
