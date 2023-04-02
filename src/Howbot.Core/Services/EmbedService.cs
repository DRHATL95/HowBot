using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Victoria;
using Victoria.Player;

namespace Howbot.Core.Services;

public class EmbedService : ServiceBase<EmbedService>, IEmbedService
{
  private readonly ILoggerAdapter<EmbedService> _logger;

  public EmbedService(ILoggerAdapter<EmbedService> logger) : base(logger)
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

  public async Task<IEmbed> GenerateMusicNowPlayingEmbedAsync(LavaTrack lavaTrack, IGuildUser user,
    ITextChannel textChannel)
  {
    if (lavaTrack == null)
    {
      throw new ArgumentNullException(nameof(lavaTrack));
    }

    if (user == null)
    {
      throw new ArgumentNullException(nameof(user));
    }

    if (textChannel == null)
    {
      throw new ArgumentNullException(nameof(textChannel));
    }

    try
    {
      var trackArtwork = await lavaTrack.FetchArtworkAsync();
      var trackDescription = lavaTrack.Position > TimeSpan.Zero
        ? $"By: {lavaTrack.Author} ({lavaTrack.Position:hh\\:mm\\:ss}/{lavaTrack.Duration:hh\\:mm\\:ss})"
        : $"By: {lavaTrack.Author} | {lavaTrack.Duration:hh\\:mm\\:ss}";

      return new EmbedBuilder()
        .WithColor(Color.DarkPurple)
        .WithTitle(":musical_note: Now Playing :musical_note:")
        .WithUrl(lavaTrack.Url)
        .WithThumbnailUrl(trackArtwork)
        .AddField(new EmbedFieldBuilder { IsInline = false, Name = lavaTrack.Title, Value = trackDescription })
        .WithFooter(GenerateEmbedFooterBuilderFromDiscordUser(user) ?? new EmbedFooterBuilder())
        .WithCurrentTimestamp()
        .Build();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(GenerateMusicNowPlayingEmbedAsync));
      _logger.LogInformation("Failed to generate music now playing embed, falling back to default embed.");
      return CreateEmbed(new EmbedOptions { Title = "Your song is now playing!" });
    }
  }

  public Task<IEmbed> GenerateMusicNextTrackEmbedAsync(Vueue<LavaTrack> queue)
  {
    ArgumentNullException.ThrowIfNull(queue);

    if (queue.Count == 0)
    {
      return Task.FromResult(CreateEmbed(new EmbedOptions { Title = "There are no more songs in the queue." }));
    }

    var nextTrack = queue.Peek();

    return Task.FromResult(CreateEmbed(new EmbedOptions
    {
      Title = "Next Track",
      Fields = new[]
      {
        new EmbedFieldBuilder
        {
          IsInline = false,
          Name = nextTrack.Title,
          Value = $"By: {nextTrack.Author} | {nextTrack.Duration:hh\\:mm\\:ss}"
        }
      }
    }));
  }

  public Task<IEmbed> GenerateMusicCurrentQueueEmbedAsync(Vueue<LavaTrack> queue)
  {
    ArgumentNullException.ThrowIfNull(queue);

    if (queue.Count == 0)
    {
      return Task.FromResult(CreateEmbed(new EmbedOptions { Title = "There are no songs in the queue." }));
    }

    var queueList = queue.ToList();
    var queueListCount = queueList.Count;
    var queueListCountString = queueListCount.ToString();

    var queueListString = queueListCount > 10
      ? string.Join("\n", queueList.Take(10).Select((track, index) => $"`{index + 1}.` {track.Title}"))
      : string.Join("\n", queueList.Select((track, index) => $"`{index + 1}.` {track.Title}"));

    return Task.FromResult(CreateEmbed(new EmbedOptions
    {
      Title = $"Current Queue ({queueListCountString})",
      Fields = new[] { new EmbedFieldBuilder { IsInline = false, Name = "Songs", Value = queueListString } }
    }));
  }

  private static EmbedFooterBuilder GenerateEmbedFooterBuilderFromDiscordUser([NotNull] IUser user)
  {
    try
    {
      var authorFooterThumbnail = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

      return new EmbedFooterBuilder()
        .WithText(user.Username)
        .WithIconUrl(authorFooterThumbnail);
    }
    catch (Exception)
    {
      // TODO: Log exception
      return new EmbedFooterBuilder();
    }
  }
}
