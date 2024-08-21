using Discord;

namespace Howbot.Core.Models;

public class EmbedOptions
{
  public EmbedOptions()
  {
    Color = Color.Default;
    Title = string.Empty;
    Url = string.Empty;
    ThumbnailUrl = string.Empty;
    Fields = null;
    Footer = new EmbedFooterBuilder();
    Author = new EmbedAuthorBuilder();
  }

  public EmbedOptions(Color color, string title, string url, string thumbnailUrl, EmbedFooterBuilder footer,
    EmbedAuthorBuilder author)
  {
    Color = color;
    Title = title;
    Url = url;
    ThumbnailUrl = thumbnailUrl;
    Footer = footer;
    Author = author;
  }

  public EmbedOptions(Color color, string title, string url, string thumbnailUrl, EmbedFieldBuilder[] fields,
    EmbedFooterBuilder footer, EmbedAuthorBuilder author)
  {
    Color = color;
    Title = title;
    Url = url;
    ThumbnailUrl = thumbnailUrl;
    Fields = fields;
    Footer = footer;
    Author = author;
  }

  public Color Color { get; }

  public string Title { get; init; }

  public string Url { get; }

  public string ThumbnailUrl { get; }

  public EmbedFieldBuilder[]? Fields { get; init; }

  public EmbedFooterBuilder Footer { get; }

  public EmbedAuthorBuilder Author { get; }
}
