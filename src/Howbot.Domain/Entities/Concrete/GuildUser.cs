using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Howbot.Domain.Entities.Abstract;

namespace Howbot.Domain.Entities.Concrete;

public class GuildUser : BaseEntity
{
  public ulong GuildId { get; set; }
  
  public ulong UserId { get; set; }
  
  public string Username { get; set; } = string.Empty;
  
  public string Discriminator { get; set; } = string.Empty;
  
  // Relationships
  public Guild? Guild { get; set; }
}
