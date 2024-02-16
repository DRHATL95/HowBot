using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Entities;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Integrations.Lavasearch;
using Lavalink4NET.Integrations.Lavasearch.Extensions;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Howbot.Core.Services;

public partial class MusicService(
  IEmbedService embedService,
  IAudioService audioService,
  IServiceProvider serviceProvider,
  ILoggerAdapter<MusicService> logger)
  : ServiceBase<MusicService>(logger), IMusicService
{
  
  [GeneratedRegex(Constants.RegexPatterns.UrlPattern)]
  private static partial Regex UrlRegex();
  
  public async ValueTask<HowbotPlayer> GetPlayerByContextAsync(SocketInteractionContext context,
    bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default,
    bool isDeferred = false, int initialVolume = 100,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    // Can't execute function without socket context
    ArgumentNullException.ThrowIfNull(context, nameof(context));

    if (context.User is not SocketGuildUser guildUser)
    {
      await context.Interaction.FollowupAsync("Unable to create player, command requested by non-guild member.");

      return null;
    }

    var voiceChannelId = guildUser.VoiceChannel?.Id ?? 0;
    IGuild guild = context.Guild;
    var guildId = guild.Id;

    var retrieveOptions = new PlayerRetrieveOptions(
      allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
      requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
      preconditions);

    float persistedVolume;

    using (var scope = serviceProvider.CreateScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
      persistedVolume = db.GetPlayerVolumeLevel(guildId);
    }

    var playerOptions = new HowbotPlayerOptions(context.Channel as ITextChannel, guildUser)
    {
      DisconnectOnDestroy = true,
      DisconnectOnStop = true,
      SelfDeaf = true,
      ClearQueueOnStop = true,
      ClearHistoryOnStop = true,
      InitialVolume = persistedVolume > 0 ? persistedVolume / 100f : initialVolume / 100f
    };

    var result = await audioService.Players.RetrieveAsync<HowbotPlayer, HowbotPlayerOptions>(guildId, voiceChannelId,
      CreatePlayerAsync,
      retrieveOptions: retrieveOptions, options: new OptionsWrapper<HowbotPlayerOptions>(playerOptions),
      cancellationToken: cancellationToken);

    if (result.IsSuccess)
    {
      return result.Player;
    }

    var errorMessage = result.Status switch
    {
      // The user is not connected to a voice channel.
      PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",

      // The bot is not in a voice channel
      PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected to a voice channel.",

      // The bot is not in the same voice channel as the user.
      PlayerRetrieveStatus.VoiceChannelMismatch => "You are not in the same voice channel as the bot.",

      // The bot failed it's precondition check.
      PlayerRetrieveStatus.PreconditionFailed => "The bot failed it's precondition check.",

      _ => "An unknown error occurred while creating the player."
    };

    await context.Interaction.FollowupAsync(errorMessage);

    return null;
  }

  public CommandResponse GetGuildMusicQueueEmbed(HowbotPlayer player)
  {
    try
    {
      if (player.Queue.Count == 0)
      {
        return CommandResponse.CommandNotSuccessful("No tracks in queue.");
      }

      var embed = embedService.GenerateMusicCurrentQueueEmbed(player.Queue);

      return CommandResponse.CommandSuccessful(embed);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(GetGuildMusicQueueEmbed));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }
  
  private ValueTask<HowbotPlayer> CreatePlayerAsync(
    IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    Guard.Against.Null(properties, nameof(properties));

    return ValueTask.FromResult(new HowbotPlayer(properties));
  }

  private async Task<List<LavalinkTrack>> GetTracksFromSearchRequestAndProviderAsync(string searchRequest, TrackSearchMode trackSearchMode)
  {
    List<LavalinkTrack> tracks = [];
    
    TrackLoadOptions trackLoadOptions = new(trackSearchMode, StrictSearchBehavior.Resolve);
    // LavaSearch categories to be returned (Tracks, Albums, Artists, Playlists, Text)
    ImmutableArray<SearchCategory> searchCategories = ImmutableArray.Create(SearchCategory.Track, SearchCategory.Playlist);

    SearchResult searchResult = null;
    
    try
    {
      searchResult = await audioService.Tracks
        .SearchAsync(searchRequest, loadOptions: trackLoadOptions, categories: searchCategories);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(GetTracksFromSearchRequestAndProviderAsync));
      
      var x = await audioService.Tracks
                            .LoadTracksAsync(searchRequest, trackLoadOptions);

      searchResult = new SearchResult(x.Tracks, [], [], [], [], ImmutableDictionary<string, JsonElement>.Empty);
    }

    if (searchResult == null || searchResult.Tracks.IsDefaultOrEmpty)
    {
      return tracks;
    }

    if (trackSearchMode == TrackSearchMode.None)
    {
      tracks.AddRange(searchResult.Tracks);
    }
    else
    {
      tracks.Add(searchResult.Tracks[0]);
    }

    return tracks;
  }
  
  #region Music Module Commands
  
  public async ValueTask<CommandResponse> PlayTrackBySearchTypeAsync(HowbotPlayer player,
    SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel)
  {
    try
    {
      TrackSearchMode trackSearchMode = UrlRegex().IsMatch(searchRequest) ? TrackSearchMode.None : LavalinkHelper.ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);
      
      var tracks = await GetTracksFromSearchRequestAndProviderAsync(searchRequest, trackSearchMode);
      if (tracks.Count == 0)
      {
        // No tracks found using LavaSearch plugin, use the default native LoadTrackAsync method
        var track = await audioService.Tracks
          .LoadTrackAsync(searchRequest, new TrackLoadOptions(trackSearchMode, StrictSearchBehavior.Resolve));
        
        if (track != null)
        {
          await player.PlayAsync(track);
          
          return CommandResponse.CommandSuccessful(track);
        }
      }
      else
      {
        var trackQueueItems = tracks.ConvertAll(track => new TrackQueueItem(new TrackReference(track)));
        
        // First enqueue the tracks
        await player.Queue.AddRangeAsync(trackQueueItems);
        
        if (player.State is PlayerState.Playing or PlayerState.Paused)
        {
          // If the player is already playing or paused, return the tracks added to queue
          return CommandResponse.CommandSuccessful($"Added {trackQueueItems.Count} tracks to queue.");
        }
        
        var track = await player.Queue.TryDequeueAsync(player.Shuffle ? TrackDequeueMode.Shuffle : TrackDequeueMode.Normal);
        if (track is null)
        {
          return CommandResponse.CommandNotSuccessful(Messages.Responses.CommandPlayNotSuccessfulResponse);
        }
        
        await player.PlayAsync(track, false);
        
        return CommandResponse.CommandSuccessful(track.Track);
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
    }

    return CommandResponse.CommandNotSuccessful(Messages.Responses.CommandPlayNotSuccessfulResponse);
  }


  public async ValueTask<CommandResponse> PauseTrackAsync(HowbotPlayer player)
  {
    try
    {
      await player.PauseAsync();

      return CommandResponse.CommandSuccessful(Messages.Responses.CommandPausedSuccessfulResponse);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(Messages.Responses.CommandPausedNotSuccessfulResponse);
    }
  }

  public async ValueTask<CommandResponse> ResumeTrackAsync(HowbotPlayer player)
  {
    try
    {
      await player.ResumeAsync();

      return CommandResponse.CommandSuccessful("Successfully resumed track.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(ResumeTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> SkipTrackAsync(HowbotPlayer player, int? numberOfTracks)
  {
    try
    {
      await player.SkipAsync(numberOfTracks ?? 1);

      return CommandResponse.CommandSuccessful($"Skipped {numberOfTracks} tracks in queue.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SkipTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> SeekTrackAsync(HowbotPlayer player, TimeSpan seekPosition)
  {
    try
    {
      Logger.LogDebug($"Seeking to {seekPosition:g}.");

      await player.SeekAsync(seekPosition);

      return CommandResponse.CommandSuccessful($"Seeking to {seekPosition:g}.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> ChangeVolumeAsync(HowbotPlayer player, int newVolume)
  {
    if (newVolume is > 1000 or < 0)
    {
      return CommandResponse.CommandNotSuccessful("Volume out of range: 0% - 1000%!");
    }

    try
    {
      await player.SetVolumeAsync(newVolume / 100f);

      using var scope = serviceProvider.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

      // Entry already exists in db
      if (db.DoesGuildExist(player.GuildId))
      {
        await db.UpdatePlayerVolumeLevel(player.GuildId, newVolume);
      }
      else
      {
        // Needs to be added to db
        db.AddNewGuild(new Guild { Id = player.GuildId, Volume = newVolume });
      }

      return CommandResponse.CommandSuccessful($"Volume set to {newVolume}%");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(ChangeVolumeAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse NowPlaying(HowbotPlayer player, IGuildUser user, ITextChannel textChannel)
  {
    try
    {
      if (player.CurrentTrack is null)
      {
        return CommandResponse.CommandNotSuccessful("No track is currently playing.");
      }

      var embed = embedService.CreateNowPlayingEmbed(new ExtendedLavalinkTrack(player.CurrentTrack), user,
        player.Position, player.Volume);

      return CommandResponse.CommandSuccessful(embed);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public ValueTask<CommandResponse> ApplyAudioFilterAsync(HowbotPlayer player, IPlayerFilters filter)
  {
    try
    {
      // Create filter options
      /*var options = new EchoFilterOptions
      {
        Delay = 1.0F,
      };

      player.Filters.Echo(options);*/

      return ValueTask.FromResult(CommandResponse.CommandSuccessful());
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return ValueTask.FromResult(CommandResponse.CommandNotSuccessful(exception));
    }
  }

  public ValueTask<CommandResponse> GetLyricsFromTrackAsync(HowbotPlayer player)
  {
    throw new NotImplementedException();
  }

  public CommandResponse ToggleShuffle(HowbotPlayer player)
  {
    try
    {
      player.Shuffle = !player.Shuffle;

      return CommandResponse.CommandSuccessful(
        $"Shuffle is now {(player.Shuffle ? "enabled" : "disabled")}.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  #endregion Music Module Commands
}
