using System.Threading.Tasks;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IDatabaseService
{
  void Initialize();

  void AddNewGuild(Guild guild);

  Guild GetGuildById(ulong guildId);

  float GetPlayerVolumeLevel(ulong guildId);

  Task UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume);

  bool DoesGuildExist(ulong guildId);
}
