using Howbot.Application.Enums;
using Howbot.Domain.Entities.Concrete;

namespace Howbot.Application.Interfaces.Infrastructure;

public interface IDatabaseService
{
  void Initialize();

  Guid? AddNewGuild(Guild guild);

  Guild? GetGuildByGuildId(ulong guildId);

  float GetPlayerVolumeLevel(ulong guildId);

  Task UpdatePlayerVolumeLevelAsync(ulong playerGuildId, float newVolume, CancellationToken cancellationToken = default);

  SearchProviderTypes GetGuildSearchProviderType(ulong guildId);

  Task UpdateSearchProviderAsync(ulong guildId, SearchProviderTypes searchProviderType);

  bool DoesGuildExist(ulong guildId);

  Task UpdateGuildPrefixAsync(ulong guildId, string newPrefix);
}
