using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Core.Services;

public class DatabaseService(IRepository repository, ILoggerAdapter<DatabaseService> logger)
  : ServiceBase<DatabaseService>(logger), IDatabaseService
{
  public void AddNewGuild(Guild guild)
  {
    try
    {
      Guard.Against.Null(guild, nameof(guild));
      
      repository.Add(guild);
    }
    catch (ArgumentNullException)
    {
      Logger.LogError("Unable to add new guild to database. Guild object is null");
    }
    catch (DbUpdateException dbUpdateException)
    {
      Logger.LogError(dbUpdateException, "Failed to add new guild to database. Likely already exists");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to add new guild to database");
    }
  }

  public Guild GetGuildById(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      return repository.GetById<Guild>(guildId);
    }
    catch (ArgumentException)
    {
      Logger.LogError("Unable to get guild by id. Invalid id provided");
      return null;
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to get guild by id {GuildId}", guildId);
      return null;
    }
  }

  public float GetPlayerVolumeLevel(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));
      
      var guildEntity = repository.GetById<Guild>(guildId);
      if (guildEntity is not null)
      {
        return guildEntity.Volume;
      }

      Logger.LogWarning("Unable to find guild with id {GuildId}", guildId);
      Logger.LogInformation("Adding new guild with id {GuildId} to database", guildId);

      AddNewGuild(new Guild { Id = guildId });

      return 0;
    }
    catch (ArgumentException)
    {
      Logger.LogError("Unable to get guild volume level from database. Invalid id provided");
      return 0;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to get guild volume level from database");
      return 0;
    }
  }

  public async Task UpdatePlayerVolumeLevel(ulong playerGuildId, float newVolume)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)playerGuildId, nameof(playerGuildId), "Invalid guild id");
      Guard.Against.NegativeOrZero((long)newVolume, nameof(newVolume), "Invalid volume level");

      var guildEntity = repository.GetById<Guild>(playerGuildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id {GuildId}", playerGuildId);
        return;
      }

      guildEntity.Volume = newVolume;

      await repository.UpdateAsync(guildEntity);
    }
    catch (ArgumentException argumentException)
    {
      Logger.LogError(argumentException, "Unable to update guild volume level. Invalid id provided");
    }
    catch (DbUpdateException dbUpdateException)
    {
      Logger.LogError(dbUpdateException, "Failed to update guild volume level");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update guild volume level");
    }
  }

  public bool DoesGuildExist(ulong guildId)
  {
    var guild = GetGuildById(guildId);

    return guild is not null;
  }
}
