using Howbot.Core.Extensions;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using Lavalink4NET.Rest.Entities.Tracks;

namespace Howbot.Core.Helpers;

public static class LavalinkHelper
{
  public static TrackSearchMode ConvertSearchProviderTypeToTrackSearchMode(SearchProviderTypes searchProviderType)
  {
    return searchProviderType switch
    {
      SearchProviderTypes.Apple => TrackSearchMode.AppleMusic,
      SearchProviderTypes.Deezer => TrackSearchMode.Deezer,
      SearchProviderTypes.SoundCloud => TrackSearchMode.SoundCloud,
      SearchProviderTypes.Spotify => TrackSearchMode.Spotify,
      SearchProviderTypes.YouTube => TrackSearchMode.YouTube,
      SearchProviderTypes.YouTubeMusic => TrackSearchMode.YouTubeMusic,
      SearchProviderTypes.YandexMusic => TrackSearchMode.YandexMusic,
      _ => TrackSearchMode.YouTube
    };
  }

  public static string GetSourceAsString(string sourceName)
  {
    if (string.IsNullOrWhiteSpace(sourceName)) { return "Unknown"; }

    // Convert to enum for easier conversion and string capitalization
    var source = EnumHelper.ConvertToEnum<LavalinkSourceNames>(sourceName);

    return source.GetDisplayName();
  }
}
