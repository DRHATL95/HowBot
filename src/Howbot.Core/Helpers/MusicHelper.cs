using System;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Howbot.Core.Helpers;

public static class MusicHelper
{
  public static bool IsSearchResponsePlaylist(SearchResponse searchResponse) =>
    !(string.IsNullOrEmpty(searchResponse.Playlist.Name));

  public static bool AreTracksSimilar(LavaTrack track, LavaTrack secondTrack)
  {
    var titleDistance = CalculateLevenshteinDistance(track.Title, secondTrack.Title);
    var authorDistance = CalculateLevenshteinDistance(track.Author, secondTrack.Author);
    var urlDistance = CalculateLevenshteinDistance(track.Url, secondTrack.Url);

    return titleDistance < 5 && authorDistance < 5 && urlDistance < 5;
  }

  private static int CalculateLevenshteinDistance(string a, string b)
  {
    var distance = new int[a.Length + 1, b.Length + 1];

    for (var i = 0; i <= a.Length; i++)
    {
      distance[i, 0] = i;
    }

    for (var j = 0; j <= b.Length; j++)
    {
      distance[0, j] = j;
    }

    for (var i = 1; i <= a.Length; i++)
    {
      for (var j = 1; j <= b.Length; j++)
      {
        var cost = b[j - 1] == a[i - 1] ? 0 : 1;

        distance[i, j] = Math.Min(Math.Min(
            distance[i - 1, j] + 1,
            distance[i, j - 1] + 1),
          distance[i - 1, j - 1] + cost);
      }
    }

    return distance[a.Length, b.Length];
  }
}
