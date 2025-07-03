using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Howbot.Domain.Entities.Abstract;

namespace Howbot.Domain.Entities.Concrete;

public class Guild : BaseEntity
{
  public ulong GuildId { get; set; }

  public string Name { get; set; } = string.Empty;

  public string Prefix { get; set; } = string.Empty;
  
  public float Volume { get; set; }

  public int SearchProvider { get; set; } = 4; // Default to YouTube

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public ICollection<GuildUser> GuildUsers { get; set; } = [];
}
