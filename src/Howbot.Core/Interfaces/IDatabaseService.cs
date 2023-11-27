using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

// Purpose: To provide a database service for the bot.
public interface IDatabaseService
{
  /// <summary>
  ///   Database initialization.
  ///   TODO: Will create the database if it doesn't exist.
  /// </summary>
  void Initialize();

  /// <summary>
  ///   Persisting a new guild to the database.
  /// </summary>
  /// <param name="guildId">The Discord Guild Id</param>
  /// <param name="prefix">The message command prefix</param>
  /// <param name="musicPlayerVolume">The audio player volume</param>
  /// <returns>The guildId or 0 for error</returns>
  ulong AddNewGuild(ulong guildId, string prefix, float musicPlayerVolume = 100f);

  /// <summary>
  ///   Get a guild by its Discord guildId
  /// </summary>
  /// <param name="guildId">The guild id where command was executed</param>
  /// <returns>Guild object or null if not able to find</returns>
  Guild GetGuildById(ulong guildId);

  /// <summary>
  ///   Returns the Guild's player volume level.
  /// </summary>
  /// <param name="guildId">The command interaction guild id</param>
  /// <returns></returns>
  int GetPlayerVolumeLevel(ulong guildId);

  /// <summary>
  ///   Persists the most current music player volume. Will be called after the command music service's change volume"/>
  /// </summary>
  /// <param name="playerGuildId">The Discord guildId used to update the Guilds table</param>
  /// <param name="newVolume">The new volume to be persisted</param>
  /// <returns>The new volume saved to database.</returns>
  float UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume);

  /// <summary>
  ///   Checks if a Guild exists in the database.
  /// </summary>
  /// <param name="guildId">The Guild to check.</param>
  /// <returns>True, if Guild exists.</returns>
  public bool DoesGuildExist(ulong guildId);
}
