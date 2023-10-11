using System;
using System.Linq;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class EmbedService : ServiceBase<EmbedService>, IEmbedService
{
  public EmbedService(ILogger<EmbedService> logger) : base(logger)
  {
  }

  public IEmbed CreateEmbed(EmbedOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

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

  public IEmbed GenerateMusicNowPlayingEmbed(LavalinkTrack track, IGuildUser user,
    ITextChannel textChannel, TimeSpan? position, float volume)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(track));
    ArgumentException.ThrowIfNullOrEmpty(nameof(user));
    ArgumentException.ThrowIfNullOrEmpty(nameof(textChannel));

    EmbedOptions defaultEmbedOptions = new EmbedOptions { Title = "Your song is now playing." };

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

  private EmbedFooterBuilder GenerateEmbedFooterBuilderFromDiscordUser([NotNull] IUser user)
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
