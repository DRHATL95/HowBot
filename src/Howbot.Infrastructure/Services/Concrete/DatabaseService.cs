using Ardalis.GuardClauses;
using Howbot.Application.Constants;
using Howbot.Application.Enums;
using Howbot.Application.Interfaces.Infrastructure;
using Howbot.Domain.Entities.Concrete;
using Howbot.Infrastructure.Services.Abstract;
using Howbot.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Services.Concrete;

public class DatabaseService(IRepository repository, ILoggerAdapter<DatabaseService> logger)
  : ServiceBase<DatabaseService>(logger), IDatabaseService
{
  public Guid? AddNewGuild(Guild guild)
  {
    try
    {
      Guard.Against.Null(guild, nameof(guild));

      var id = repository.Add(guild).Id;

      return id;
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

    return null;
  }

  public Guild? GetGuildByGuildId(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      // If the guild does not exist, create a new one and persist to the database
      if (!ShouldCreateGuild(guildId))
      {
        return repository.GetGuildByGuildId(guildId);
      }

      Logger.LogInformation("Adding new guild with id [{GuildId}] to database", guildId);

      var id = AddNewGuild(new Guild
      {
        GuildId = guildId,
        Prefix = BotDefaults.DefaultPrefix,
        Volume = BotDefaults.DefaultVolume,
        SearchProvider = (int)BotDefaults.DefaultSearchProvider
      });

      if (!id.HasValue)
      {
        Logger.LogError("Failed to add new guild with id [{GuildId}] to database", guildId);
        return null;
      }

      return repository.GetById<Guild>(id.Value);
    }
    catch (ArgumentException)
    {
      Logger.LogError("Unable to get guild by id. Invalid id provided");
      return null;
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to get guild by id [{GuildId}]", guildId);
      return null;
    }
  }

  public float GetPlayerVolumeLevel(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      var guildEntity = repository.GetGuildByGuildId(guildId);
      if (guildEntity is not null)
      {
        return guildEntity.Volume;
      }

      Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
      Logger.LogInformation("Adding new guild with id [{GuildId}] to database", guildId);

      AddNewGuild(new Guild { GuildId = guildId });

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

  public async Task UpdatePlayerVolumeLevelAsync(ulong playerGuildId, float newVolume, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    try
    {
      Guard.Against.NegativeOrZero((long)playerGuildId, nameof(playerGuildId), "Invalid guild id");
      Guard.Against.NegativeOrZero((long)newVolume, nameof(newVolume), "Invalid volume level");

      var guildEntity = repository.GetGuildByGuildId(playerGuildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", playerGuildId);
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

  public SearchProviderTypes GetGuildSearchProviderType(ulong guildId)
  {
    try
    {
      if (guildId <= 0)
      {
        throw new ArgumentException("Invalid guild id");
      }

      var guildEntity = repository.GetGuildByGuildId(guildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return SearchProviderTypes.YouTubeMusic;
      }

      return (SearchProviderTypes)guildEntity.SearchProvider;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to get search provider type for guild [{GuildId}]", guildId);
      throw;
    }
  }

  public async Task UpdateSearchProviderAsync(ulong guildId, SearchProviderTypes searchProviderType)
  {
    try
    {
      if (guildId <= 0)
      {
        throw new ArgumentException("Invalid guild id");
      }

      var guildEntity = repository.GetGuildByGuildId(guildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return;
      }

      guildEntity.SearchProvider = (int)searchProviderType;

      await repository.UpdateAsync(guildEntity);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update search provider for guild [{GuildId}]", guildId);
      throw;
    }
  }

  public bool DoesGuildExist(ulong guildId)
  {
    var guild = GetGuildByGuildId(guildId);

    return guild is not null;
  }

  public async Task UpdateGuildPrefixAsync(ulong guildId, string newPrefix)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(newPrefix, nameof(newPrefix));

      var guildEntity = repository.GetGuildByGuildId(guildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return;
      }

      guildEntity.Prefix = newPrefix;

      await repository.UpdateAsync(guildEntity);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update guild prefix for guild [{GuildId}]", guildId);
      throw;
    }
  }

  private bool ShouldCreateGuild(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      return repository.GetGuildByGuildId(guildId) is null;
    }
    catch (ArgumentException)
    {
      Logger.LogError("Unable to check if guild should be created. Invalid id provided");
      return false;
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to check if guild should be created");
      return false;
    }
  }
}
