using System;
using Howbot.Core.Entities;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class DatabaseService : ServiceBase<DatabaseService>, IDatabaseService
{
  private readonly IRepository _repository;

  public DatabaseService(IRepository repository)
  {
    _repository = repository;
  }

  public override void Initialize()
  {
    if (Logger.IsEnabled(LogLevel.Debug))
    {
      Logger.LogDebug("Initializing {ServiceName}...", nameof(DatabaseService));
    }
  }

  public ulong AddNewGuild(ulong guildId, string prefix, float musicPlayerVolume = 100.0f)
  {
    ArgumentException.ThrowIfNullOrEmpty(prefix);

    var guildEntity = new Guild
    {
      Id = guildId, Prefix = prefix ?? "!~", MusicVolumeLevel = (int)Math.Round(musicPlayerVolume)
    };

    try
    {
      var entity = _repository.Add(guildEntity);

      return entity.Id;
    }
    catch (DbUpdateException dbUpdateException)
    {
      Logger.LogError(dbUpdateException, "Failed to add new guild to database. Likely already exists.");
      return 0;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to add new guild to database.");
      return 0;
    }
  }

  public Guild GetGuildById(ulong guildId)
  {
    ParameterHelper.EnsureNotNullAndZeroOrGreater(guildId, nameof(guildId));

    try
    {
      return _repository.GetById<Guild>(guildId);
    }
    catch (Exception e)
    {
      HandleException(e);
    }

    return null;
  }

  public float GetPlayerVolumeLevel(ulong guildId)
  {
    ParameterHelper.EnsureNotNullAndZeroOrGreater(guildId, nameof(guildId));

    try
    {
      var guildEntity = _repository.GetById<Guild>(guildId);

      if (guildEntity == null)
      {
        Logger.LogWarning("Unable to find guild with id {GuildId}", guildId);
        return 0.0f;
      }

      Logger.LogDebug("Player volume from db: {volume}", guildEntity.MusicVolumeLevel);
      Logger.LogDebug("Returned value: {volume}", guildEntity.MusicVolumeLevel / 100.0f);

      return guildEntity.MusicVolumeLevel / 100.0f;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to get guild volume level from database.");
      throw;
    }
  }

  public float UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume)
  {
    ParameterHelper.EnsureNotNullAndZeroOrGreater(playerGuildId, nameof(playerGuildId));
    ParameterHelper.EnsureNotNullAndZeroOrGreater(newVolume, nameof(newVolume));

    try
    {
      var guildEntity = _repository.GetById<Guild>(playerGuildId);

      if (guildEntity == null)
      {
        Logger.LogWarning("Unable to find guild with id {GuildId}", playerGuildId);
        return 0.0f;
      }

      guildEntity.MusicVolumeLevel = (int)Math.Round(newVolume);

      // Persist
      _repository.Update(guildEntity);

      return newVolume;
    }
    catch (DbUpdateException dbUpdateException)
    {
      Logger.LogError(dbUpdateException, "Failed to update guild volume level.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update guild volume level.");
    }

    return 0.0f;
  }
}
