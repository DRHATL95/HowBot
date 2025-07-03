using Howbot.Application.Enums;
using Howbot.Domain.Entities.Concrete;

namespace Howbot.Application.Interfaces.Infrastructure;

public interface IDatabaseService
{
  void Initialize();

  void AddNewGuild(Guild guild);

  Guild? GetGuildById(ulong guildId);

  float GetPlayerVolumeLevel(ulong guildId);

  Task UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume);

  SearchProviderTypes GetSearchProviderTypeAsync(ulong guildId);

  Task UpdateSearchProviderAsync(ulong guildId, SearchProviderTypes searchProviderType);

  bool DoesGuildExist(ulong guildId);

  Task UpdateGuildPrefixAsync(ulong guildId, string newPrefix);
}
