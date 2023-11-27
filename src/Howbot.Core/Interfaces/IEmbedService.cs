using System;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for EmbedService for creating embed replies and messages
public interface IEmbedService
{
  /// <summary>
  ///   Initializes the embed service.
  /// </summary>
  void Initialize();

  /// <summary>
  ///   Helper method to create an embed.
  /// </summary>
  /// <param name="embedOptions">Embed options used to create embed</param>
  /// <returns>The embed created from <see cref="EmbedOptions" /></returns>
  IEmbed CreateEmbed(EmbedOptions embedOptions);

  /// <summary>
  ///   Generates an embed of the current track playing in the guild.
  /// </summary>
  /// <param name="queueItem"></param>
  /// <param name="user"></param>
  /// <param name="textChannel"></param>
  /// <param name="position"></param>
  /// <param name="volume"></param>
  /// <returns></returns>
  IEmbed GenerateMusicNowPlayingEmbed(LavalinkTrack queueItem, IGuildUser user,
    ITextChannel textChannel, TimeSpan? position, float volume);

  /// <summary>
  ///   Peeks at the next track in the queue and generates an embed of the next track.
  /// </summary>
  /// <param name="queue"></param>
  /// <returns></returns>
  IEmbed GenerateMusicNextTrackEmbed(ITrackQueue queue);

  /// <summary>
  ///   Generates an embed of the current music queue, up to 10 tracks.
  /// </summary>
  /// <param name="queue">The current guild music queue</param>
  /// <returns>An embed of the songs in queue up to 10 tracks.</returns>
  IEmbed GenerateMusicCurrentQueueEmbed(ITrackQueue queue);
}
