using System;
using Ardalis.GuardClauses;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Core.Services;

public class DatabaseService : ServiceBase<DatabaseService>, IDatabaseService
{
  private readonly IRepository _repository;

  public DatabaseService(IRepository repository, ILoggerAdapter<DatabaseService> logger) : base(logger)
  {
    _repository = repository;
  }

  public override void Initialize()
  {
    // TODO: Check EF Core if database has been created
    Logger.LogDebug("Initializing {ServiceName}...", nameof(DatabaseService));
  }

  public ulong AddNewGuild(ulong guildId, string prefix, float musicPlayerVolume = 100.0f)
  {
    Guard.Against.NullOrEmpty(prefix, nameof(prefix));

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
      Logger.LogError(dbUpdateException, "Failed to add new guild to database. Likely already exists");
      return 0;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to add new guild to database");
      return 0;
    }
  }

  public Guild GetGuildById(ulong guildId)
  {
    Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

    try
    {
      return _repository.GetById<Guild>(guildId);
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to get guild by id {GuildId}", guildId);
      return null;
    }
  }

  public int GetPlayerVolumeLevel(ulong guildId)
  {
    Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

    try
    {
      var guildEntity = _repository.GetById<Guild>(guildId);
      if (guildEntity is not null)
      {
        return guildEntity.MusicVolumeLevel;
      }

      Logger.LogWarning("Unable to find guild with id {GuildId}", guildId);
      return 0;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to get guild volume level from database");
      return 0;
    }
  }

  public float UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume)
  {
    Guard.Against.NegativeOrZero((long)playerGuildId, nameof(playerGuildId));
    Guard.Against.NegativeOrZero((long)newVolume, nameof(newVolume));

    try
    {
      var guildEntity = _repository.GetById<Guild>(playerGuildId);

      if (guildEntity == null)
      {
        Logger.LogWarning("Unable to find guild with id {GuildId}", playerGuildId);
        return 0.0f;
      }

      guildEntity.MusicVolumeLevel = (int)Math.Round(newVolume);

      _repository.Update(guildEntity);

      return newVolume;
    }
    catch (DbUpdateException dbUpdateException)
    {
      Logger.LogError(dbUpdateException, "Failed to update guild volume level");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update guild volume level");
    }

    return 0.0f;
  }

  public bool DoesGuildExist(ulong guildId)
  {
    var guild = GetGuildById(guildId);

    return guild is not null;
  }
}
