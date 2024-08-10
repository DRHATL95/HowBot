using Howbot.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(IServiceProvider serviceProvider) : Controller
{
  [HttpGet("player/{guildId}")]
  public IActionResult Index(ulong guildId)
  {
    using var scope = serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

    var sessionId = db.GetGuildSessionId(guildId);
    
    return Ok(sessionId);
  }
}
