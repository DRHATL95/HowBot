using Howbot.Infrastructure.Services;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Spotify;

public record Album
{
  [JsonProperty("album_type")] public string AlbumType { get; set; } = string.Empty;

  [JsonProperty("artists")] public List<Artist> Artists { get; set; } = new();

  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("images")] public List<Image> Images { get; set; } = new();

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("release_date")] public string ReleaseDate { get; set; } = string.Empty;

  [JsonProperty("release_date_precision")]
  public string ReleaseDatePrecision { get; set; } = string.Empty;

  [JsonProperty("total_tracks")] public int TotalTracks { get; set; }

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}
