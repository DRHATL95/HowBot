using System;
using Victoria.Responses.Search;

namespace Howbot.Core.Helpers;

public static class MusicHelper
{
  public static bool IsSearchResponsePlaylist(SearchResponse searchResponse) => !(string.IsNullOrEmpty(searchResponse.Playlist.Name));
}
