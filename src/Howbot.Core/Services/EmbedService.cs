using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Services;

public class EmbedService : ServiceBase<EmbedService>, IEmbedService
{
  [NotNull] private readonly ILoggerAdapter<EmbedService> _logger;

  public EmbedService([NotNull] ILoggerAdapter<EmbedService> logger) : base(logger)
  {
    _logger = logger;
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

  public ValueTask<IEmbed> GenerateMusicNowPlayingEmbedAsync(LavalinkTrack track, IGuildUser user, ITextChannel textChannel)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(track));
    ArgumentException.ThrowIfNullOrEmpty(nameof(user));
    ArgumentException.ThrowIfNullOrEmpty(nameof(textChannel));

    EmbedOptions defaultEmbedOptions = new EmbedOptions { Title = "Your song is now playing." };

    Uri trackArtworkUri = track.ArtworkUri;

    try
    {
      var embed = new EmbedBuilder()
        .WithColor(Color.Default)
        .WithTitle("Now Playing")
        .WithUrl(track.Uri?.AbsoluteUri ?? string.Empty)
        .WithDescription($"{track.Title} - {track.Author}")
        .WithThumbnailUrl(trackArtworkUri?.AbsoluteUri ?? string.Empty)
        .WithFooter(GenerateEmbedFooterBuilderFromDiscordUser(user))
        .WithCurrentTimestamp()
        .Build();

      return ValueTask.FromResult((IEmbed)embed);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(GenerateMusicNowPlayingEmbedAsync));
      _logger.LogInformation("Failed to generate music now playing embed, falling back to default embed.");
      return ValueTask.FromResult(CreateEmbed(defaultEmbedOptions));
    }
  }

  public ValueTask<IEmbed> GenerateMusicNextTrackEmbedAsync(ITrackQueue queue)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(queue));

    if (!queue.TryPeek(out ITrackQueueItem nextTrackItem))
    {
      return ValueTask.FromResult(CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." }));
    }

    if (nextTrackItem?.Track is null)
    {
      return ValueTask.FromResult(CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." }));
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

    return ValueTask.FromResult(embed);

  }

  public ValueTask<IEmbed> GenerateMusicCurrentQueueEmbedAsync(ITrackQueue queue)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(queue));

    if (queue.IsEmpty)
    {
      return ValueTask.FromResult(CreateEmbed(new EmbedOptions { Title = "There are no songs in the queue." }));
    }

    var queueList = queue.Count > 10
      ? string.Join("\n", queue.Take(10).Select(((item, i) => $"`{i + 1}.` {item.Identifier}")))
      : string.Join(Environment.NewLine, queue.Select(((item, i) => $"`{i + 1}.` {item.Identifier}")));

    var embed = CreateEmbed(new EmbedOptions()
    {
      Title = $"Current Queue ({queue.Count})",
      Fields = new[] { new EmbedFieldBuilder() { IsInline = false, Name = "Songs", Value = queueList } }
    });

    return ValueTask.FromResult(embed);
  }

  [NotNull]
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
      _logger.LogError(exception);
      return new EmbedFooterBuilder();
    }
  }

}
