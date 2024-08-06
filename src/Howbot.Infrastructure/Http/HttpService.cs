using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Commands;
using Howbot.Core.Settings;
using Newtonsoft.Json;
using Constants = Howbot.Infrastructure.Data.Config.Constants;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Howbot.Infrastructure.Http;

public partial class HttpService(IHttpClientFactory httpClientFactory) : IHttpService, IDisposable
{
  private readonly HttpClient _client = httpClientFactory.CreateClient();

  public void Dispose()
  {
    _client.Dispose();

    GC.SuppressFinalize(this);
  }

  /// <summary>
  ///   Gets the response status code of the specified URL.
  /// </summary>
  /// <param name="url">The URL to send the request to.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>The response status code as an integer.</returns>
  public async Task<int> GetUrlResponseStatusCodeAsync(string url, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var result = await _client.GetAsync(url, cancellationToken);

    return (int)result.StatusCode;
  }

  /// <summary>
  ///   Creates a Watch2Gether room for watching videos together.
  /// </summary>
  /// <param name="url">The URL of the video to be shared.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>The URL of the created Watch2Gether room.</returns>
  public async Task<string> CreateWatchTogetherRoomAsync(string url, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var parameters = new Watch2GetherParameters
    {
      W2GApiKey = Configuration.WatchTogetherApiKey,
      Share = url,
      BackgroundColor = "#00ff00",
      BackgroundOpacity = "50"
    };

    var request = new HttpRequestMessage
    {
      RequestUri = new Uri(Constants.WatchTogetherCreateRoomUrl),
      Method = HttpMethod.Post,
      Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
      Content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json")
    };

    var httpResponseMessage = await _client.SendAsync(request, cancellationToken);
    httpResponseMessage.EnsureSuccessStatusCode();

    var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
    var convertedResponse = JsonSerializer.Deserialize<Watch2GetherResponse>(response) ??
                            throw new Exception("Unable to parse response");

    return $"{Constants.WatchTogetherRoomUrl}/{convertedResponse.StreamKey}";
  }

  /// <summary>
  ///   TODO
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public async Task<List<ActivityApplication>> GetCurrentApplicationIdsAsync(
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    const string versionPattern = @"###\s*(.+)";
    var data = new List<ActivityApplication>();

    const string url = "https://raw.githubusercontent.com/Delitefully/DiscordLists/master/activities.md";

    var result = await _client.GetAsync(url, cancellationToken);
    if (!result.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to download data from {url}. Status code: {result.StatusCode}");
    }

    var content = await result.Content.ReadAsStringAsync(cancellationToken);
    var lines = content.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries);

    var version = string.Empty;

    foreach (var line in lines)
    {
      var versionMatch = Regex.Match(line, versionPattern);
      if (versionMatch.Success)
      {
        version = versionMatch.Groups[1].Value;
      }

      var match = DiscordApplicationIdsLineRegex().Match(line);
      if (match.Success)
      {
        data.Add(new ActivityApplication
        {
          Version = version,
          IconUrl = match.Groups[1].Value,
          Id = Convert.ToUInt64(match.Groups[2].Value),
          Name = match.Groups[3].Value.Trim(),
          MaxParticipants = match.Groups[4].Value.Trim()
        });
      }
    }

    return data;
  }

  /// <summary>
  ///   TODO
  /// </summary>
  /// <param name="channelId"></param>
  /// <param name="activityId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public async Task<string> StartDiscordActivityAsync(string channelId, string activityId,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var requestUri = $"https://discord.com/api/v9/channels/{channelId}/invites";

    var requestContent = new StringContent(JsonSerializer.Serialize(new
    {
      max_age = 86400,
      max_uses = 10,
      target_application_id = activityId,
      target_type = 2,
      temporary = false,
      validate = false // null?
    }), Encoding.UTF8, "application/json");

    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Configuration.DiscordToken);

    var response = await _client.PostAsync(requestUri, requestContent, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to start activity. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    var invite = JsonSerializer.Deserialize<DiscordInvite>(responseContent) ??
                 throw new Exception($"Failed to start activity. Invite is null. Response: {responseContent}");

    // Return the invite link
    return $"https://discord.gg/{invite.Code}";
  }

  /// <summary>
  ///   TODO
  /// </summary>
  /// <param name="itemName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public async Task<Tuple<string, string, int>?> GetTarkovMarketPriceByItemNameAsync(string itemName,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var query =
      $"{{items(name: \"{itemName}\") {{id name shortName basePrice wikiLink avg24hPrice iconLink updated sellFor {{price currency priceRUB source}}}}}}";
    var data = new Dictionary<string, string> { { "query", query } };

    const string url = Core.Models.Constants.EscapeFromTarkov.EftApiBaseUrl;

    var response = await _client.PostAsJsonAsync(url, data, cancellationToken);

    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (!response.IsSuccessStatusCode || responseContent.Contains("errors"))
    {
      throw new Exception(
        $"Failed to get Tarkov market price. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    var rawResponse = JsonSerializer.Deserialize<GraphQlResponse>(responseContent) ??
                      throw new Exception("Unable to parse tarkov API response");

    if (rawResponse.Data.Items.Count == 0 || rawResponse.Data.Items.All(x => x.SellFor.Count == 0))
    {
      return null;
    }

    var dictionary = new Dictionary<string, int>();

    foreach (var item in rawResponse.Data.Items)
    {
      var splitName = item.Name.Split(' ');
      if (splitName.Contains(itemName, StringComparer.OrdinalIgnoreCase))
      {
        dictionary[item.Name] = 0;
      }
      else
      {
        dictionary[item.Name] = StringHelper.CalculateLevenshteinDistance(itemName, item.Name);
      }
    }

    var result = dictionary.OrderBy(kvp => kvp.Value).FirstOrDefault();

    if (result.Key is null)
    {
      return null;
    }

    var marketItem = rawResponse.Data.Items.FirstOrDefault(x => x.Name == result.Key);

    if (marketItem is null)
    {
      return null;
    }

    // Get the highest price and the trader
    var maxPrice = 0; // Rubles
    var trader = string.Empty;

    foreach (var item in marketItem.SellFor.Where(x => x is { PriceInRubles: > 0, Price: > 0 }))
    {
      if (item.PriceInRubles <= maxPrice)
      {
        continue;
      }

      maxPrice = item.Price;
      trader = item.Source;
    }

    if (maxPrice == 0 || string.IsNullOrEmpty(trader))
    {
      return null;
    }

    return new Tuple<string, string, int>(result.Key, trader, maxPrice);
  }

  // This is for application ids, this will provide one line at a time from the markdown file
  // https://raw.githubusercontent.com/Delitefully/DiscordLists/master/activities.md
  [GeneratedRegex(@"\|\s*!\[Icon\]\((.*?)\)\s*\|\s*(\d{18})\s*\|\s*([^|]+?)\s*\|\s*([^|]+?)\s*\|")]
  private static partial Regex DiscordApplicationIdsLineRegex();
}

internal record GraphQlResponse
{
  [JsonProperty("data")] public Data Data { get; set; } = new();
}

internal record Data
{
  [JsonProperty("items")] public List<TarkovMarketItem> Items { get; set; } = [];
}

internal record TarkovMarketItem
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("shortName")] public string ShortName { get; set; } = string.Empty;

  [JsonProperty("basePrice")] public string BasePrice { get; set; } = string.Empty;

  [JsonProperty("wikiLink")] public string WikiLink { get; set; } = string.Empty;

  [JsonProperty("avg24hPrice")] public string Avg24HPrice { get; set; } = string.Empty;

  [JsonProperty("iconLink")] public string IconLink { get; set; } = string.Empty;

  [JsonProperty("updated")] public string Updated { get; set; } = string.Empty;

  [JsonProperty("sellFor")] public List<TarkovSellForRequest> SellFor { get; set; } = [];
}

internal record TarkovSellForRequest
{
  [JsonProperty("price")] public int Price { get; set; }

  [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;

  [JsonProperty("priceRUB")] public int PriceInRubles { get; set; }

  [JsonProperty("source")] public string Source { get; set; } = string.Empty;
}

internal record DiscordInvite
{
  public string Code { get; set; } = string.Empty;
}

internal record Watch2GetherParameters
{
  [JsonProperty("w2g_api_key")] public string W2GApiKey { get; set; } = string.Empty;

  [JsonProperty("share")] public string Share { get; set; } = string.Empty;

  [JsonProperty("bg_color")] public string BackgroundColor { get; set; } = string.Empty;

  [JsonProperty("bg_opacity")] public string BackgroundOpacity { get; set; } = string.Empty;
}

internal record Watch2GetherResponse
{
  [JsonProperty("id")] public int Id { get; set; }

  [JsonProperty("streamkey")] public string StreamKey { get; set; } = string.Empty;

  [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

  [JsonProperty("persistent")] public bool Persistent { get; set; }

  [JsonProperty("persistent_name")] public string PersistentName { get; set; } = string.Empty;

  [JsonProperty("deleted")] public bool Deleted { get; set; }

  [JsonProperty("moderated")] public bool Moderated { get; set; }

  [JsonProperty("location")] public string Location { get; set; } = string.Empty;

  [JsonProperty("stream_created")] public bool StreamCreated { get; set; }

  [JsonProperty("background")] public string Background { get; set; } = string.Empty;

  [JsonProperty("moderated_background")] public bool ModeratedBackground { get; set; }

  [JsonProperty("moderated_playlist")] public bool ModeratedPlaylist { get; set; }

  [JsonProperty("bg_color")] public string BackgroundColor { get; set; } = string.Empty;

  [JsonProperty("bg_opacity")] public double BackgroundOpacity { get; set; }

  [JsonProperty("moderated_item")] public bool ModeratedItem { get; set; }

  [JsonProperty("theme_bg")] public string ThemeBackground { get; set; } = string.Empty;

  [JsonProperty("playlist_id")] public int PlaylistId { get; set; }

  [JsonProperty("members_only")] public bool MembersOnly { get; set; }

  [JsonProperty("moderated_suggestions")]
  public bool ModeratedSuggestions { get; set; }

  [JsonProperty("moderated_chat")] public bool ModeratedChat { get; set; }

  [JsonProperty("moderated_user")] public bool ModeratedUser { get; set; }

  [JsonProperty("moderated_cam")] public bool ModeratedCam { get; set; }
}

internal record SpotifyRecommendationsResponse
{
  [JsonProperty("tracks")] public List<SpotifyTrack> Tracks { get; set; } = new();

  [JsonProperty("seeds")] public List<SpotifySeed> Seeds { get; set; } = new();
}

internal record SpotifyTrack
{
  [JsonProperty("album")] public SpotifyAlbum Album { get; set; } = new();

  [JsonProperty("artists")] public List<SpotifyArtist> Artists { get; set; } = new();

  [JsonProperty("duration_ms")] public int DurationMs { get; set; }

  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("popularity")] public int Popularity { get; set; }

  [JsonProperty("preview_url")] public string PreviewUrl { get; set; } = string.Empty;

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}

internal record SpotifyAlbum
{
  [JsonProperty("album_type")] public string AlbumType { get; set; } = string.Empty;

  [JsonProperty("artists")] public List<SpotifyArtist> Artists { get; set; } = new();

  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("images")] public List<SpotifyImage> Images { get; set; } = new();

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("release_date")] public string ReleaseDate { get; set; } = string.Empty;

  [JsonProperty("release_date_precision")]
  public string ReleaseDatePrecision { get; set; } = string.Empty;

  [JsonProperty("total_tracks")] public int TotalTracks { get; set; }

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}

internal record SpotifyArtist
{
  [JsonProperty("external_urls")] public Dictionary<string, string> ExternalUrls { get; set; } = new();

  [JsonProperty("href")] public string Href { get; set; } = string.Empty;

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string Name { get; set; } = string.Empty;

  [JsonProperty("type")] public string Type { get; set; } = string.Empty;

  [JsonProperty("uri")] public string Uri { get; set; } = string.Empty;
}

internal record SpotifyImage
{
  [JsonProperty("height")] public int Height { get; set; }

  [JsonProperty("url")] public string Url { get; set; } = string.Empty;

  [JsonProperty("width")] public int Width { get; set; }
}

internal record SpotifySeed
{
  [JsonProperty("initialPoolSize")] public int InitialPoolSize { get; set; }

  [JsonProperty("afterFilteringSize")] public int AfterFilteringSize { get; set; }

  [JsonProperty("afterRelinkingSize")] public int AfterRelinkingSize { get; set; }

  [JsonProperty("href")] public string Href { get; set; } = string.Empty;

  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("type")] public string Type { get; set; } = string.Empty;
}
