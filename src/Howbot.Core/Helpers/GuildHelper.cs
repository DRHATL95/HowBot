using Discord;

namespace Howbot.Core.Helpers;

public static class GuildHelper
{
  public static string GetGuildTag(IGuild guild)
  {
    return guild == null ? string.Empty : $"[{guild.Name} - {guild.Id}]";
  }
}
