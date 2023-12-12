using System;
using System.Linq;
using Ardalis.GuardClauses;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Services;

/// <summary>
///   Service class for creating and generating embeds.
/// </summary>
public class EmbedService(ILoggerAdapter<EmbedService> logger) : ServiceBase<EmbedService>(logger), IEmbedService
{
  /// <summary>
  ///   Creates an embed using the provided embed options.
  /// </summary>
  /// <param name="options">The options used to configure the embed.</param>
  /// <returns>The created embed.</returns>
  public IEmbed CreateEmbed(EmbedOptions options)
  {
    Guard.Against.Null(options, nameof(options));

    var embedBuilder = new EmbedBuilder { Color = options.Color };

    if (!string.IsNullOrWhiteSpace(options.Title))
    {
      embedBuilder.Title = options.Title;
    }

    if (!string.IsNullOrWhiteSpace(options.Url))
    {
      embedBuilder.Url = options.Url;
    }

    if (!string.IsNullOrWhiteSpace(options.ThumbnailUrl))
    {
      embedBuilder.ThumbnailUrl = options.ThumbnailUrl;
    }

    if (options.Fields is { Length: > 0 })
    {
      embedBuilder.Fields = options.Fields.ToList();
    }

    if (options.Footer != null)
    {
      embedBuilder.Footer = options.Footer;
    }

    if (options.Author != null)
    {
      embedBuilder.Author = options.Author;
    }

    embedBuilder.WithCurrentTimestamp();

    return embedBuilder.Build();
  }

  /// <summary>
  ///   Generates an embed for the currently playing music.
  /// </summary>
  /// <param name="track">The currently playing track.</param>
  /// <param name="user">The user playing the track.</param>
  /// <param name="textChannel">The text channel where the track is being played.</param>
  /// <param name="position">The current position of the track.</param>
  /// <param name="volume">The volume of the track.</param>
  /// <returns>The generated embed for displaying the now playing music.</returns>
  public IEmbed GenerateMusicNowPlayingEmbed(LavalinkTrack track, IGuildUser user,
    ITextChannel textChannel, TimeSpan? position, float volume)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(track));
    ArgumentException.ThrowIfNullOrEmpty(nameof(user));
    ArgumentException.ThrowIfNullOrEmpty(nameof(textChannel));

    EmbedOptions defaultEmbedOptions = new EmbedOptions { Title = "Your song is now playing." };

    // TODO: Replace with image downloaded within project.
    Uri trackArtworkUri = track.ArtworkUri ?? new Uri("https://i.imgur.com/Lf76JRO.png");

    try
    {
      var embedBuilder = new EmbedBuilder()
        .WithColor(Color.Default)
        .WithTitle($"{track.Title} - {track.Author}")
        .WithUrl(track.Uri?.AbsoluteUri ?? string.Empty)
        .WithThumbnailUrl(trackArtworkUri?.AbsoluteUri)
        .WithFooter(GenerateEmbedFooterBuilderFromDiscordUser(user))
        .WithCurrentTimestamp();

      if (!string.IsNullOrEmpty(track.SourceName))
      {
        embedBuilder.Fields.Add(new EmbedFieldBuilder()
        {
          IsInline = true, Name = "Source", Value = LavalinkHelper.GetSourceAsString(track.SourceName)
        });
      }

      if (position.HasValue)
      {
        embedBuilder.Fields.Add(new EmbedFieldBuilder()
        {
          IsInline = true,
          Name = "Position",
          Value =
            $@"{position.Value:hh\:mm\:ss}/{track.Duration:hh\:mm\:ss}"
        });
      }

      embedBuilder.Fields.Add(new EmbedFieldBuilder { IsInline = true, Name = "Volume", Value = $"{volume * 100}%" });

      var embed = embedBuilder.Build();

      return embed;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(GenerateMusicNowPlayingEmbed));
      return CreateEmbed(defaultEmbedOptions);
    }
  }

  /// <summary>
  ///   Generates an embed for the next track in the given track queue.
  /// </summary>
  /// <param name="queue">The track queue to generate the embed for.</param>
  /// <returns>An embed representing the next track in the queue.</returns>
  public IEmbed GenerateMusicNextTrackEmbed(ITrackQueue queue)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(queue));

    if (!queue.TryPeek(out ITrackQueueItem nextTrackItem))
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." });
    }

    if (nextTrackItem?.Track is null)
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." });
    }

    var embed = CreateEmbed(new EmbedOptions()
    {
      Title = "Next Track In Queue",
      Fields = new[]
      {
        new EmbedFieldBuilder()
        {
          IsInline = false,
          Name = nextTrackItem.Identifier,
          Value = $@"By: {nextTrackItem.Track.Author} | {nextTrackItem.Track.Duration:hh\:mm\:ss}"
        }
      }
    });

    return embed;
  }

  /// <summary>
  ///   Generates an embed to display the current music queue.
  /// </summary>
  /// <param name="queue">The music queue to generate the embed for.</param>
  /// <returns>An embed containing the current music queue information.</returns>
  public IEmbed GenerateMusicCurrentQueueEmbed(ITrackQueue queue)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(queue));

    if (queue.IsEmpty)
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no songs in the queue." });
    }

    var queueList = queue.Count > 10
      ? string.Join("\n", queue.Take(10).Select(((item, i) => $"`{i + 1}.` {item.Identifier}")))
      : string.Join(Environment.NewLine, queue.Select(((item, i) => $"`{i + 1}.` {item.Identifier}")));

    var embed = CreateEmbed(new EmbedOptions()
    {
      Title = $"Current Queue ({queue.Count})",
      Fields = new[] { new EmbedFieldBuilder() { IsInline = false, Name = "Songs", Value = queueList } }
    });

    return embed;
  }

  /// <summary>
  ///   Generates an EmbedFooterBuilder object from a Discord user.
  /// </summary>
  /// <param name="user">The Discord user.</param>
  /// <returns>An EmbedFooterBuilder object.</returns>
  private EmbedFooterBuilder GenerateEmbedFooterBuilderFromDiscordUser(IUser user)
  {
    try
    {
      var authorFooterThumbnail = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

      return new EmbedFooterBuilder()
        .WithText(user.Username)
        .WithIconUrl(authorFooterThumbnail);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(GenerateEmbedFooterBuilderFromDiscordUser));
      return new EmbedFooterBuilder();
    }
  }
}
