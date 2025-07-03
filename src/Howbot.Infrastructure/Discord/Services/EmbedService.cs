using Ardalis.GuardClauses;
using Discord;
using Howbot.Application.Constants;
using Howbot.Application.Interfaces.Discord;
using Howbot.Application.Models.Discord;
using Howbot.Infrastructure.Audio.Helpers;
using Howbot.Infrastructure.Services.Abstract;
using Howbot.SharedKernel;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

namespace Howbot.Infrastructure.Discord.Services;

public class EmbedService(ILoggerAdapter<EmbedService> logger) : ServiceBase<EmbedService>(logger), IEmbedService
{
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

  public IEmbed GenerateMusicNextTrackEmbed(ITrackQueue queue)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(queue));

    if (!queue.TryPeek(out var nextTrackItem))
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." });
    }

    if (nextTrackItem?.Track is null)
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." });
    }

    var embed = CreateEmbed(new EmbedOptions
    {
      Title = "Next Track In Queue",
      Fields = new[]
      {
        new EmbedFieldBuilder
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
    Guard.Against.Null(queue, nameof(queue));

    if (queue.IsEmpty)
    {
      return CreateEmbed(new EmbedOptions { Title = "There are no songs in the queue." });
    }

    var queueList = queue.Count > 10
      ? string.Join("\n", queue.Take(10).Select((item, i) => $"`{i + 1}.` {item.Track?.Title ?? "No Track Title"}"))
      : string.Join(Environment.NewLine,
        queue.Select((item, i) => $"`{i + 1}.` {item.Track?.Title ?? "No Track Title"}"));

    var embed = CreateEmbed(new EmbedOptions
    {
      Title = $"Current Queue ({queue.Count})",
      Fields = new[] { new EmbedFieldBuilder { IsInline = false, Name = "Songs", Value = queueList } }
    });

    return embed;
  }

  public IEmbed CreateNowPlayingEmbed(ExtendedLavalinkTrack lavalinkTrack, IUser user, TrackPosition? trackPosition,
    float volume)
  {
    var embedBuilder = GenerateEmbedBuilderForNowPlaying(lavalinkTrack, user);

    embedBuilder.Fields.Add(new EmbedFieldBuilder
    {
      IsInline = true,
      Name = "Source",
      Value = LavalinkHelper.GetSourceAsString(lavalinkTrack.SourceName ?? string.Empty)
    });

    if (trackPosition.HasValue)
    {
      embedBuilder.Fields.Add(new EmbedFieldBuilder
      {
        IsInline = true,
        Name = "Position",
        Value = $@"{trackPosition.Value.Position:hh\:mm\:ss} / {lavalinkTrack.Duration:hh\:mm\:ss}"
      });
    }

    embedBuilder.Fields.Add(new EmbedFieldBuilder { IsInline = true, Name = "Volume", Value = $"{volume * 100}%" });

    return embedBuilder.Build();
  }

  public IEmbed CreateNowPlayingEmbed(ExtendedLavalinkTrack lavalinkTrack)
  {
    var embedBuilder = new EmbedBuilder()
      .WithDescription($"{Emojis.Speaker} Now Playing: **{lavalinkTrack.Title}**")
      .WithColor(Embeds.ThemeColor);

    return embedBuilder.Build();
  }

  public IEmbed CreateTrackAddedToQueueEmbed(ExtendedLavalinkTrack lavalinkTrack, IUser user)
  {
    var embedBuilder = new EmbedBuilder
    {
      Color = Embeds.ThemeColor,
      Description = $"{Emojis.MusicalNote} Added **{lavalinkTrack.Title}** to the queue."
    };

    return embedBuilder.Build();
  }

  private EmbedBuilder GenerateEmbedBuilderForNowPlaying(ExtendedLavalinkTrack lavalinkTrack, IUser user)
  {
    var embedBuilder = new EmbedBuilder()
      .WithTitle(lavalinkTrack.Title)
      .WithUrl(lavalinkTrack.Track.Uri?.AbsoluteUri ?? string.Empty)
      .WithThumbnailUrl(lavalinkTrack.ArtworkUri?.AbsoluteUri ?? string.Empty)
      .WithColor(Embeds.ThemeColor)
      .WithFooter(GenerateEmbedFooterBuilderFromDiscordUser(user))
      .WithCurrentTimestamp();

    return embedBuilder;
  }

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
