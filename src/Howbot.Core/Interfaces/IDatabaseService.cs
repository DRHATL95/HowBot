using JetBrains.Annotations;

namespace Howbot.Core.Interfaces;

public interface IDatabaseService
{
  void Initialize();

  ulong AddNewGuild(ulong guildId, [CanBeNull] string prefix, int musicPlayerVolume = 100);
}
