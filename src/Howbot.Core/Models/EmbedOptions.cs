using Discord;
using JetBrains.Annotations;

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

  public Color Color { get; set; }

  public string Title { get; set; }

  public string Url { get; set; }

  public string ThumbnailUrl { get; set; }

  public EmbedFieldBuilder[] Fields { get; set; }

  public EmbedFooterBuilder Footer { get; set; }

  public EmbedAuthorBuilder Author { get; set; }
}
