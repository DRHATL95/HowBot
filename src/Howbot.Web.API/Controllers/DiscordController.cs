using Discord.Rest;
using Howbot.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Howbot.Web.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiscordController(DiscordRestClient discordRestClient) : Controller
{
  [HttpGet]
  public IActionResult Index()
  {
    return Ok("Discord API controller is working!");
  }

  [HttpGet("user/{userId}/guilds")]
  public async Task<IActionResult> GetGuildsForUserId(ulong userId)
  {
    try
    {
      var user = await discordRestClient.GetUserAsync(userId);
      var guilds = await discordRestClient.GetGuildsAsync();

      var userGuilds = await Task.WhenAll(guilds.Select(async guild =>
      {
        var users = await guild.SearchUsersAsync(user.Username);
        return new { Guild = guild, UserCount = users.Count };
      }));

      var filteredGuilds = userGuilds.Where(g => g.UserCount != 0).Select(g => g.Guild);

      return Ok(filteredGuilds.Select(g => new GuildDto()
      {
        Id = g.Id,
        Name = g.Name,
        Icon = g.IconUrl,
        Owner = g.OwnerId == userId,
        // Permissions = (int)g.Permissions.RawValue
      }));
    }
    catch (Exception e)
    {
      BadRequest(e.Message);
    }

    return BadRequest("Unable to find Guild for User");
  }

  [HttpGet("guild/{guildId}")]
  public async Task<IActionResult> GetGuildById(ulong guildId)
  {
    try
    {
      var guild = await discordRestClient.GetGuildAsync(guildId);
      return Ok(new GuildDto()
      {
        Id = guild.Id, Name = guild.Name, Icon = guild.IconUrl, Owner = guild.OwnerId == guildId,
        // Permissions = (int)guild.Permissions.RawValue
      });
    }
    catch (Exception e)
    {
      BadRequest(e.Message);
    }

    return BadRequest("Unable to find Guild");
  }
}
