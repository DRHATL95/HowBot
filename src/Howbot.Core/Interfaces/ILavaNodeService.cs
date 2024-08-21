namespace Howbot.Core.Interfaces;

public interface ILavaNodeService
{
  void Initialize();

  Task<string> GetSessionIdForGuildAsync(ulong guildId);
}
