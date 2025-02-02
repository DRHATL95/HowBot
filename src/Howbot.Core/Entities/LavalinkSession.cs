namespace Howbot.Core.Entities;

public class LavalinkSession : BaseEntity
{
  public string EncryptedSessionId { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  // Foreign Key to Guilds table
  public ulong GuildId { get; set; }
}
