using Howbot.Core.Entities;
using Howbot.Core.Models.Enums;

namespace Howbot.Core.Interfaces;

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

  string GetGuildSessionId(ulong guildId);

  Task UpdateGuildSessionIdAsync(ulong guildId, string sessionId);
}
