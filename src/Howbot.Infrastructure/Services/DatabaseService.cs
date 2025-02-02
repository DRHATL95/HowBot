using Ardalis.GuardClauses;
using Howbot.Core.Entities;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Services;

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

  public Guild? GetGuildById(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      // If the guild does not exist, create a new one and persist to the database
      if (!ShouldCreateGuild(guildId))
      {
        return repository.GetById<Guild>(guildId);
      }

      Logger.LogInformation("Adding new guild with id [{GuildId}] to database", guildId);

      AddNewGuild(new Guild
      {
        Id = guildId,
        Prefix = Constants.DefaultPrefix,
        Volume = Constants.DefaultVolume,
        SearchProvider = Constants.DefaultSearchProvider
      });

      return repository.GetById<Guild>(guildId);
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

      var guildEntity = repository.GetById<Guild>(guildId);
      if (guildEntity is not null)
      {
        return guildEntity.Volume;
      }

      Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
      Logger.LogInformation("Adding new guild with id [{GuildId}] to database", guildId);

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

  public SearchProviderTypes GetSearchProviderTypeAsync(ulong guildId)
  {
    try
    {
      if (guildId <= 0)
      {
        throw new ArgumentException("Invalid guild id");
      }

      var guildEntity = repository.GetById<Guild>(guildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return SearchProviderTypes.YouTubeMusic;
      }

      return guildEntity.SearchProvider;
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

      var guildEntity = repository.GetById<Guild>(guildId);
      if (guildEntity is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return;
      }

      guildEntity.SearchProvider = searchProviderType;

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
    var guild = GetGuildById(guildId);

    return guild is not null;
  }

  public async Task UpdateGuildPrefixAsync(ulong guildId, string newPrefix)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(newPrefix, nameof(newPrefix));

      var guildEntity = repository.GetById<Guild>(guildId);
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

  public string GetGuildSessionId(ulong guildId)
  {
    var guild = repository.GetById<Guild>(guildId);
    if (guild is null)
    {
      Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
      return string.Empty;
    }

    var sessions = repository.List<LavalinkSession>();
    if (sessions is null || sessions.Count == 0)
    {
      Logger.LogWarning("No sessions found in database");
      return string.Empty;
    }

    var session = sessions.FirstOrDefault(s => s.Id == guildId);

    return session is null 
      ? string.Empty 
      : StringCipher.Decrypt(session.EncryptedSessionId, Infrastructure.Data.Config.Constants.EncryptionKey);
  }

/*  public async Task UpdateSessionIdAsync(ulong sessionId, string newSessionId)
  {
    try
    {
      var session = repository.GetById<LavalinkSession>(sessionId);
      if (session is null)
      {
        Logger.LogWarning("Unable to find session id [{SessionId}]", sessionId);
        return;
      }

      var encryptedSessionId = StringCipher.Encrypt(newSessionId, Infrastructure.Data.Config.Constants.EncryptionKey);

      session.EncryptedSessionId = encryptedSessionId;

      await repository.UpdateAsync(session);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update session id [{SessionId}]", sessionId);
      throw;
    }
  }*/

  public async Task UpdateSessionIdAsync(ulong guildId, string sessionId)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(sessionId, nameof(sessionId));

      var guild = repository.GetById<Guild>(guildId);
      if (guild is null)
      {
        Logger.LogWarning("Unable to find guild with id [{GuildId}]", guildId);
        return;
      }

      var sessions = repository.List<LavalinkSession>();
      if (sessions is null || sessions.Count == 0)
      {
        Logger.LogWarning("No sessions found in database");
        return;
      }

      var session = sessions.FirstOrDefault(s => s.Id == guildId);
      if (session is null)
      {
        Logger.LogWarning("Unable to find session for guild with id [{GuildId}]", guildId);
        return;
      }

      var encryptedSessionId = StringCipher.Encrypt(sessionId, Infrastructure.Data.Config.Constants.EncryptionKey);

      session.EncryptedSessionId = encryptedSessionId;

      await repository.UpdateAsync(session);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to update guild session id for guild [{GuildId}]", guildId);
      throw;
    }
  }

  private bool ShouldCreateGuild(ulong guildId)
  {
    try
    {
      Guard.Against.NegativeOrZero((long)guildId, nameof(guildId));

      return repository.GetById<Guild>(guildId) is null;
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
