using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Howbot.Domain.Entities.Concrete;
using Howbot.Infrastructure.Data;

namespace Howbot.Presentation.WebAPI.Controllers;

  [Route("api/[controller]")]
  [ApiController]
  public class GuildsController : ControllerBase
  {
      private readonly AppDbContext _context;

      public GuildsController(AppDbContext context)
      {
          _context = context;
      }

      // GET: api/Guilds
      [HttpGet]
      public async Task<ActionResult<IEnumerable<Guild>>> GetGuilds()
      {
          return await _context.Guilds.ToListAsync();
      }

      // GET: api/Guilds/5
      [HttpGet("{id}")]
      public async Task<ActionResult<Guild>> GetGuild(Guid id)
      {
          var guild = await _context.Guilds.FindAsync(id);

          if (guild == null)
          {
              return NotFound();
          }

          return guild;
      }

      // PUT: api/Guilds/5
      // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
      [HttpPut("{id}")]
      public async Task<IActionResult> PutGuild(Guid id, Guild guild)
      {
          if (id != guild.Id)
          {
              return BadRequest();
          }

          _context.Entry(guild).State = EntityState.Modified;

          try
          {
              await _context.SaveChangesAsync();
          }
          catch (DbUpdateConcurrencyException)
          {
              if (!GuildExists(id))
              {
                  return NotFound();
              }
              else
              {
                  throw;
              }
          }

          return NoContent();
      }

      // POST: api/Guilds
      // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
      [HttpPost]
      public async Task<ActionResult<Guild>> PostGuild(Guild guild)
      {
          _context.Guilds.Add(guild);
          await _context.SaveChangesAsync();

          return CreatedAtAction("GetGuild", new { id = guild.Id }, guild);
      }

      // DELETE: api/Guilds/5
      [HttpDelete("{id}")]
      public async Task<IActionResult> DeleteGuild(Guid id)
      {
          var guild = await _context.Guilds.FindAsync(id);
          if (guild == null)
          {
              return NotFound();
          }

          _context.Guilds.Remove(guild);
          await _context.SaveChangesAsync();

          return NoContent();
      }

      private bool GuildExists(Guid id)
      {
          return _context.Guilds.Any(e => e.Id == id);
      }
  }
