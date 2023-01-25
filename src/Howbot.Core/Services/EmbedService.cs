using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Victoria;
using Victoria.Player;

namespace Howbot.Core.Services;

public class EmbedService : IEmbedService
{
  private readonly ILoggerAdapter<EmbedService> _logger;

  public EmbedService(ILoggerAdapter<EmbedService> logger)
  {
    _logger = logger;
  }
  
  public async Task<IEmbed> GenerateMusicNowPlayingEmbedAsync(LavaTrack lavaTrack, IGuildUser user, ITextChannel textChannel)
  {
    if (lavaTrack == null) throw new ArgumentNullException(nameof(lavaTrack));
    if (user == null) throw new ArgumentNullException(nameof(user));
    if (textChannel == null) throw new ArgumentNullException(nameof(textChannel));

    try
    {
      var trackArtwork = await lavaTrack.FetchArtworkAsync();
      var trackDescription = (lavaTrack.Position > TimeSpan.Zero) ?
        $"By: {lavaTrack.Author} ({lavaTrack.Position:hh\\:mm\\:ss}/{lavaTrack.Duration:hh\\:mm\\:ss})" :
        $"By: {lavaTrack.Author} | {lavaTrack.Duration:hh\\:mm\\:ss}";

      return new EmbedBuilder()
        .WithColor(Color.DarkPurple)
        .WithTitle(":musical_note: Now Playing :musical_note:")
        .WithUrl(lavaTrack.Url)
        .WithThumbnailUrl(trackArtwork)
        .AddField(new EmbedFieldBuilder() { IsInline = false, Name = lavaTrack.Title, Value = trackDescription})
        .WithFooter(GenerateEmbedFooterBuilderFromDiscordUser(user) ?? new EmbedFooterBuilder())
        .WithCurrentTimestamp()
        .Build();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in EmbedService.GenerateMusicNowPlayingEmbedAsync");
      throw;
    }
  }

  public Task<IEmbed> GenerateMusicNextTrackEmbedAsync()
  {
    throw new System.NotImplementedException();
  }

  public Task<IEmbed> GenerateMusicCurrentQueueEmbedAsync()
  {
    throw new System.NotImplementedException();
  }

  private static EmbedFooterBuilder GenerateEmbedFooterBuilderFromDiscordUser([NotNull]IUser user)
  {
    var authorFooterThumbnail = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

    return new EmbedFooterBuilder()
      .WithText(user.Username)
      .WithIconUrl(authorFooterThumbnail);
  }

  public void Initialize()
  {
    // TODO:
  }
}
