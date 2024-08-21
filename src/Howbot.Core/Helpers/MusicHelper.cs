using Ardalis.GuardClauses;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Helpers;

public static class MusicHelper
{
  public static bool AreTracksSimilar(LavalinkTrack lavalinkTrack, LavalinkTrack secondLavalinkTrack)
  {
    Guard.Against.Null(lavalinkTrack, nameof(lavalinkTrack));
    Guard.Against.Null(secondLavalinkTrack, nameof(secondLavalinkTrack));

    var titleDistance = StringHelper.CalculateLevenshteinDistance(lavalinkTrack.Title, secondLavalinkTrack.Title);
    var authorDistance = StringHelper.CalculateLevenshteinDistance(lavalinkTrack.Author, secondLavalinkTrack.Author);
    var urlDistance =
      StringHelper.CalculateLevenshteinDistance(lavalinkTrack.Identifier, secondLavalinkTrack.Identifier);

    return titleDistance < 5 && authorDistance < 5 && urlDistance < 5;
  }
}
