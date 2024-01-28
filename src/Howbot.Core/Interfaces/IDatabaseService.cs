using System.Threading.Tasks;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IDatabaseService
{
  void Initialize();

  /// <summary>
  ///   Persisting a new guild to the database.
  /// </summary>
  /// <param name="guild">The guild to be persisted</param>
  void AddNewGuild(Guild guild);

  /// <summary>
  ///   Get a guild by its Discord guildId
  /// </summary>
  /// <param name="guildId">The guild id where command was executed</param>
  /// <returns>Guild object or null if not able to find</returns>
  Guild GetGuildById(ulong guildId);

  /// <summary>
  ///   Returns the Guild music player volume level.
  /// </summary>
  /// <param name="guildId">The Discord guildId used to query the Guilds table</param>
  /// <returns>The volume level</returns>
  float GetPlayerVolumeLevel(ulong guildId);

  /// <summary>
  ///   Persists the most current music player volume. Will be called after the command music service's change volume"/>
  /// </summary>
  /// <param name="playerGuildId">The Discord guildId used to update the Guilds table</param>
  /// <param name="newVolume">The new volume to be persisted</param>
  Task UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume);

  /// <summary>
  ///   Checks if a Guild exists in the database.
  /// </summary>
  /// <param name="guildId">The Guild to check.</param>
  /// <returns>True, if Guild exists.</returns>
  bool DoesGuildExist(ulong guildId);
}
