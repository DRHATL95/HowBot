﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Integrations.Lavasearch;
using Lavalink4NET.Integrations.Lavasearch.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Howbot.Core.Services;

public class MusicService(
  IEmbedService embedService,
  IAudioService audioService,
  IServiceProvider serviceProvider,
  ILoggerAdapter<MusicService> logger)
  : ServiceBase<MusicService>(logger), IMusicService
{
  /// <summary>
  ///   Retrieves or creates a player for the given socket interaction context.
  /// </summary>
  /// <param name="context">The socket interaction context.</param>
  /// <param name="allowConnect">Indicates whether the player can join the voice channel if necessary. Default is false.</param>
  /// <param name="requireChannel">Indicates whether the player requires a voice channel. Default is true.</param>
  /// <param name="preconditions">The preconditions for the player. Default is empty.</param>
  /// <param name="isDeferred">Indicates whether the player is deferred. Default is false.</param>
  /// <param name="initialVolume">The initial volume of the player. Default is 100.</param>
  /// <param name="cancellationToken">The cancellation token. Default is empty.</param>
  /// <returns>The retrieved or created HowbotPlayer instance, or null if an error occurs.</returns>
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
      await context.Interaction.FollowupAsync("Unable to create player, command requested by non-guild member.")
        .ConfigureAwait(false);

      return null;
    }

    ulong voiceChannelId = guildUser.VoiceChannel?.Id ?? 0;
    IGuild guild = context.Guild;
    ulong guildId = guild.Id;

    var retrieveOptions = new PlayerRetrieveOptions(
      ChannelBehavior: allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
      VoiceStateBehavior: requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
      Preconditions: preconditions);

    int persistedVolume;

    using (var scope = serviceProvider.CreateScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
      persistedVolume = db.GetPlayerVolumeLevel(guildId);
    }

    HowbotPlayerOptions playerOptions = new HowbotPlayerOptions()
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
        cancellationToken: cancellationToken)
      .ConfigureAwait(false);

    if (!result.IsSuccess)
    {
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

      await context.Interaction.FollowupAsync(errorMessage).ConfigureAwait(false);

      return null;
    }

    return result.Player;
  }

  /// <summary>
  ///   TODO:
  /// </summary>
  /// <param name="videoId"></param>
  /// <param name="count"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public ValueTask<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1)
  {
    throw new NotImplementedException();
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

  /// <summary>
  ///   Creates a new player asynchronously.
  /// </summary>
  /// <param name="properties">The properties of the player to be created.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>
  ///   A <see cref="ValueTask{TResult}" /> representing the asynchronous operation. The result of the task will be
  ///   the created <see cref="HowbotPlayer" /> instance.
  /// </returns>
  private static ValueTask<HowbotPlayer> CreatePlayerAsync(
    IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    ArgumentNullException.ThrowIfNull(properties);

    Log.Logger.Information("Creating new player..");

    return ValueTask.FromResult(new HowbotPlayer(properties));
  }

  #region Music Module Commands

  /// <summary>
  ///   Plays a track by search type asynchronously.
  /// </summary>
  /// <param name="player">The HowbotPlayer instance.</param>
  /// <param name="searchProviderType">The search provider type.</param>
  /// <param name="searchRequest">The search request.</param>
  /// <param name="user">The IGuildUser to play the track for.</param>
  /// <param name="voiceState">The IVoiceState of the user.</param>
  /// <param name="textChannel">The ITextChannel where the command is executed.</param>
  /// <returns>A ValueTask of type CommandResponse.</returns>
  public async ValueTask<CommandResponse> PlayTrackBySearchTypeAsync(HowbotPlayer player,
    SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel)
  {
    try
    {
      // Convert from enum to Lavalink struct for searching providers (default is YouTube)
      var type = LavalinkHelper.ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);

      var trackOptions = new TrackLoadOptions(type);

      // LavaSearch categories to be returned (Tracks, Albums, Artists, etc.)
      var categories = ImmutableArray.Create(SearchCategory.Track);

      var searchResult = await audioService.Tracks
        .SearchAsync(searchRequest, loadOptions: trackOptions, categories: categories)
        .ConfigureAwait(false);

      LavalinkTrack track;
      if (searchResult is null || searchResult.Tracks.IsDefaultOrEmpty)
      {
        // Attempts to use native lavalink native search when lava search plugin isn't working or doesn't return results for categories specified
        track = await audioService.Tracks
          .LoadTrackAsync(searchRequest, trackOptions)
          .ConfigureAwait(false);
      }
      else
      {
        track = searchResult.Tracks[0];
      }

      if (track != null)
      {
        await player.PlayAsync(track).ConfigureAwait(false);

        return CommandResponse.CommandSuccessful(track);
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
      Logger.LogDebug("[{GuildId}] - Pausing current track.", player.GuildId);

      await player.PauseAsync().ConfigureAwait(false);

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
      await player.ResumeAsync().ConfigureAwait(false);

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
      if (player is IQueuedLavalinkPlayer { State: PlayerState.Playing } queuedPlayer)
      {
        await queuedPlayer.SkipAsync(numberOfTracks ?? 1);

        return CommandResponse.CommandSuccessful($"Skipped {numberOfTracks} tracks in queue.");
      }

      return CommandResponse.CommandNotSuccessful("Unable to skip to position in queue.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Exception thrown in MusicService.SkipTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> SeekTrackAsync(HowbotPlayer player, TimeSpan seekPosition)
  {
    try
    {
      Logger.LogDebug($"Seeking to {seekPosition:g}.");

      await player.SeekAsync(seekPosition).ConfigureAwait(false);

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
      await player.SetVolumeAsync(newVolume / 100f).ConfigureAwait(false);

      using var scope = serviceProvider.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

      // Entry already exists in db
      if (db.DoesGuildExist(player.GuildId))
      {
        db.UpdatePlayerVolumeLevel(player.GuildId, newVolume);
      }
      else
      {
        // Needs to be added to db
        db.AddNewGuild(player.GuildId, Constants.DefaultPrefix, newVolume);
      }

      return CommandResponse.CommandSuccessful($"Volume set to {newVolume}%");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse NowPlaying(HowbotPlayer player, IGuildUser user,
    ITextChannel textChannel)
  {
    try
    {
      if (player.CurrentTrack is null)
      {
        return CommandResponse.CommandNotSuccessful("No track is currently playing.");
      }

      var embed = embedService.GenerateMusicNowPlayingEmbed(player.CurrentTrack, user, textChannel,
        player.Position?.Position, player.Volume);

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

  public CommandResponse ToggleTwoFourSeven(HowbotPlayer player)
  {
    throw new NotImplementedException();
  }

  #endregion Music Module Commands
}
