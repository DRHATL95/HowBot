using Howbot.Core.Settings;
using Lavalink4NET.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MusicController(ILavalinkApiClientFactory lavalinkRestClient) : Controller
{
  [HttpGet("{guildId}")]
  public async Task<IActionResult> Index(ulong guildId)
  {
    var client = lavalinkRestClient.Create(Options.Create<LavalinkApiClientOptions>(new LavalinkApiClientOptions()
    {
      Passphrase = Configuration.AudioServiceOptions.Passphrase,
      BaseAddress = Configuration.AudioServiceOptions.BaseAddress
    }));

    var serverInfo = await client.RetrieveServerInformationAsync();
    
    return Ok(serverInfo);
  }
}
