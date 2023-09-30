using JetBrains.Annotations;

namespace Howbot.Core.Entities;

public class Guild : BaseEntity
{
  [CanBeNull] public string Prefix { get; set; }
  public int GuildMusicVolumeLevel { get; set; }
}
