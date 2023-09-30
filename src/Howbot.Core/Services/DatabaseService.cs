using System;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
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
      Logger.LogDebug("Initializing DatabaseService...");
    }

    var guildId = AddNewGuild(656305202185633810, null);
    Logger.LogInformation($"Added new guild with id {guildId} to database.");
  }

  public ulong AddNewGuild(ulong guildId, string prefix, int musicPlayerVolume = 100)
  {
    var guildEntity = new Guild { Id = guildId, Prefix = prefix ?? "!~", GuildMusicVolumeLevel = musicPlayerVolume };

    try
    {
      var entity = _repository.Add(guildEntity);

      return entity.Id;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Failed to add new guild to database.");
      throw;
    }
  }
}
