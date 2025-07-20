using Howbot.Application.Enums;

namespace Howbot.Application.Interfaces.Discord;

public interface IGuildSettingsService
{
  ValueTask<int> GetGuildVolumeAsync(string guildId, CancellationToken token = default);
  ValueTask<bool> SetGuildVolumeAsync(string guildId, int volume, CancellationToken token = default);
  ValueTask<SearchProviderTypes> GetGuildSearchProviderAsync(string guildId, CancellationToken token = default);
  
}
