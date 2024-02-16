using System;
using Ardalis.GuardClauses;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Helpers;

public static class MusicHelper
{
  public static bool AreTracksSimilar(LavalinkTrack lavalinkTrack, LavalinkTrack secondLavalinkTrack)
  {
    Guard.Against.Null(lavalinkTrack, nameof(lavalinkTrack));
    Guard.Against.Null(secondLavalinkTrack, nameof(secondLavalinkTrack));

    var titleDistance = CalculateLevenshteinDistance(lavalinkTrack.Title, secondLavalinkTrack.Title);
    var authorDistance = CalculateLevenshteinDistance(lavalinkTrack.Author, secondLavalinkTrack.Author);
    var urlDistance = CalculateLevenshteinDistance(lavalinkTrack.Identifier, secondLavalinkTrack.Identifier);

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
