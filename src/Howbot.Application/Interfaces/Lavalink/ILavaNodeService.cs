namespace Howbot.Application.Interfaces.Lavalink;

public interface ILavaNodeService
{
  void Initialize();

  Task<string> GetSessionIdForGuildAsync(ulong guildId);
}
