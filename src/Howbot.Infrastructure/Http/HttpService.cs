using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Settings;
using Newtonsoft.Json;
using Constants = Howbot.Infrastructure.Data.Config.Constants;

namespace Howbot.Infrastructure.Http;

/// <summary>
///   An implementation of IHttpService using HttpClient
/// </summary>
public class HttpService : IHttpService
{
  /// <summary>
  ///   Gets the response status code of the specified URL.
  /// </summary>
  /// <param name="url">The URL to send the request to.</param>
  /// <returns>The response status code as an integer.</returns>
  public async Task<int> GetUrlResponseStatusCodeAsync(string url)
  {
    using var client = new HttpClient();

    var result = await client.GetAsync(url);

    return (int)result.StatusCode;
  }

  /// <summary>
  ///   Creates a Watch2Gether room for watching videos together.
  /// </summary>
  /// <param name="url">The URL of the video to be shared.</param>
  /// <returns>The URL of the created Watch2Gether room.</returns>
  public async Task<string> CreateWatchTogetherRoomAsync(string url)
  {
    using var client = new HttpClient();

    var parameters = new Watch2GetherParameters
    {
      W2GApiKey = Configuration.WatchTogetherApiKey,
      Share = url,
      BackgroundColor = "#00ff00",
      BackgroundOpacity = "50"
    };

    var convertedParams = JsonConvert.SerializeObject(parameters);

    var request = new HttpRequestMessage
    {
      RequestUri = new Uri(Constants.WatchTogetherCreateRoomUrl),
      Method = HttpMethod.Post,
      Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
      Content = new StringContent(convertedParams, Encoding.UTF8, "application/json")
    };

    var httpResponseMessage = await client.SendAsync(request);
    if (!httpResponseMessage.IsSuccessStatusCode)
    {
      return string.Empty;
    }

    var response = httpResponseMessage.Content.ReadAsStringAsync().Result;
    var convertedResponse = JsonConvert.DeserializeObject<Watch2GetherResponse>(response);

    return $"{Constants.WatchTogetherRoomUrl}/{convertedResponse.StreamKey}";
  }

  public async Task<List<ActivityApplication>> GetCurrentApplicationIdsAsync(CancellationToken token = default)
  {
    token.ThrowIfCancellationRequested();

    const string rowPattern = @"\|\s*!\[Icon\]\((.*?)\)\s*\|\s*(\d{18})\s*\|\s*([^|]+?)\s*\|\s*([^|]+?)\s*\|";
    const string versionPattern = @"###\s*(.+)";

    var data = new List<ActivityApplication>();

    const string url = "https://raw.githubusercontent.com/Delitefully/DiscordLists/master/activities.md";
    using var client = new HttpClient();

    var result = await client.GetAsync(url, token);
    if (!result.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to download data from {url}. Status code: {result.StatusCode}");
    }

    var content = await result.Content.ReadAsStringAsync(token);
    var lines = content.Split(["\n", "\r\n"], StringSplitOptions.RemoveEmptyEntries);

    var version = string.Empty;

    foreach (var line in lines)
    {
      var versionMatch = Regex.Match(line, versionPattern);
      if (versionMatch.Success)
      {
        // Should only define version once at beginning of table. I.e. Stable, Development, Staging, etc.
        version = versionMatch.Groups[1].Value;
      }

      var match = Regex.Match(line, rowPattern);
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

  public async Task<string> StartDiscordActivityAsync(string channelId, string activityId)
  {
    var requestUri = $"https://discord.com/api/v9/channels/{channelId}/invites";

    var requestContent = new StringContent(JsonConvert.SerializeObject(new
    {
      max_age = 86400,
      max_uses = 10,
      target_application_id = activityId,
      target_type = 2,
      temporary = false,
      validate = false // null?
    }), Encoding.UTF8, "application/json");

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Configuration.DiscordToken);

    var response = await client.PostAsync($"https://discord.com/api/v8/channels/{channelId}/invites", requestContent);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to start activity. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    var invite = JsonConvert.DeserializeObject<DiscordInvite>(responseContent);

    // Return the invite link
    return $"https://discord.gg/{invite.Code}";
  }
  
  /*public async Task<Tuple<string, string, int>> GetTarkovMarketPriceByItemNameAsync(string itemName)
  {
    using var client = new HttpClient();
    
    var query = $"{{items(name: \"{itemName}\") {{id name shortName basePrice wikiLink avg24hPrice iconLink sellFor {{price currency priceRUB source}}}}}}";
    var data = new Dictionary<string, string> { { "query", query } };

    const string url = Core.Models.Constants.EscapeFromTarkov.EftApiBaseUrl;
    
    var response = await client.PostAsJsonAsync(url, data);
    
    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode || responseContent.Contains("errors"))
    {
      throw new Exception($"Failed to get Tarkov market price. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    var rawResponse = JsonConvert.DeserializeObject<GraphQlResponse>(responseContent);
    
    if (rawResponse.Data.Items.Count == 0 || rawResponse.Data.Items.All(x => x.SellFor.Count == 0))
    {
      return null;
    }

    Dictionary<string, int> dictionary = new Dictionary<string, int>();

    foreach (var item in rawResponse.Data.Items)
    {
      dictionary[item.Name] = StringHelper.CalculateLevenshteinDistance(itemName, item.Name);
    }
    
    // var result = dictionary.Where(kvp => kvp.Key.EndsWith(itemName, StringComparison.OrdinalIgnoreCase)).OrderBy(kvp => kvp.Value).FirstOrDefault();
    var result = dictionary.OrderBy(kvp => kvp.Value).FirstOrDefault();
    
    if (result.Key is null) return null;
    
    var marketItem = rawResponse.Data.Items.FirstOrDefault(x => x.Name == result.Key);
    
    // Get the highest price and the trader
    int maxPrice = 0; // Rubles
    string trader = string.Empty;
    
    foreach (var item in marketItem.SellFor.Where(x => x is { PriceInRubles: > 0, Price: > 0 }))
    {
      if (item.PriceInRubles <= maxPrice)
      {
        continue;
      }

      maxPrice = item.Price;
      trader = item.Source;
    }
    
    if (maxPrice == 0 || string.IsNullOrEmpty(trader)) return null;
    
    return new Tuple<string, string, int>(result.Key, trader, maxPrice);
  }*/
  
  public async Task<Tuple<string, string, int>> GetTarkovMarketPriceByItemNameAsync(string itemName)
{
    using var client = new HttpClient();

    var query = $"{{items(name: \"{itemName}\") {{id name shortName basePrice wikiLink avg24hPrice iconLink updated sellFor {{price currency priceRUB source}}}}}}";
    var data = new Dictionary<string, string> { { "query", query } };

    const string url = Core.Models.Constants.EscapeFromTarkov.EftApiBaseUrl;

    var response = await client.PostAsJsonAsync(url, data);

    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode || responseContent.Contains("errors"))
    {
        throw new Exception($"Failed to get Tarkov market price. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    var rawResponse = JsonConvert.DeserializeObject<GraphQlResponse>(responseContent);

    if (rawResponse.Data.Items.Count == 0 || rawResponse.Data.Items.All(x => x.SellFor.Count == 0))
    {
        return null;
    }

    Dictionary<string, int> dictionary = new Dictionary<string, int>();

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

    if (result.Key is null) return null;

    var marketItem = rawResponse.Data.Items.FirstOrDefault(x => x.Name == result.Key);

    // Get the highest price and the trader
    int maxPrice = 0; // Rubles
    string trader = string.Empty;

    foreach (var item in marketItem.SellFor.Where(x => x is { PriceInRubles: > 0, Price: > 0 }))
    {
        if (item.PriceInRubles <= maxPrice)
        {
            continue;
        }

        maxPrice = item.Price;
        trader = item.Source;
    }

    if (maxPrice == 0 || string.IsNullOrEmpty(trader)) return null;

    return new Tuple<string, string, int>(result.Key, trader, maxPrice);
}
}

internal struct GraphQlResponse
{
  [JsonProperty("data")] 
  public Data Data { get; set; }
}

internal struct Data
{
  [JsonProperty("items")]
  public List<TarkovMarketItem> Items { get; set; }
}

internal struct TarkovMarketItem
{
  [JsonProperty("id")]
  public string Id { get; set; }
  
  [JsonProperty("name")]
  public string Name { get; set; }
  
  [JsonProperty("shortName")]
  public string ShortName { get; set; }
  
  [JsonProperty("basePrice")]
  public string BasePrice { get; set; }
  
  [JsonProperty("wikiLink")]
  public string WikiLink { get; set; }
  
  [JsonProperty("avg24hPrice")]
  public string Avg24hPrice { get; set; }
  
  [JsonProperty("iconLink")]
  public string IconLink { get; set; }
  
  [JsonProperty("updated")]
  public string Updated { get; set; }
  
  [JsonProperty("sellFor")]
  public List<TarkovSellForRequest> SellFor { get; set; }
}

internal struct TarkovSellForRequest
{
  [JsonProperty("price")]
  public int Price { get; set; }
  
  [JsonProperty("currency")]
  public string Currency { get; set; }
  
  [JsonProperty("priceRUB")]
  public int PriceInRubles { get; set; }
  
  [JsonProperty("source")]
  public string Source { get; set; }
}

internal struct DiscordInvite
{
  public string Code { get; set; }
}

internal struct Watch2GetherParameters
{
  [JsonProperty("w2g_api_key")] public string W2GApiKey { get; set; }

  [JsonProperty("share")] public string Share { get; set; }

  [JsonProperty("bg_color")] public string BackgroundColor { get; set; }

  [JsonProperty("bg_opacity")] public string BackgroundOpacity { get; set; }
}

internal struct Watch2GetherResponse
{
  [JsonProperty("id")] public int Id { get; set; }

  [JsonProperty("streamkey")] public string StreamKey { get; set; }

  [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

  [JsonProperty("persistent")] public bool Persistent { get; set; }

  [JsonProperty("persistent_name")] public string PersistentName { get; set; }

  [JsonProperty("deleted")] public bool Deleted { get; set; }

  [JsonProperty("moderated")] public bool Moderated { get; set; }

  [JsonProperty("location")] public string Location { get; set; }

  [JsonProperty("stream_created")] public bool StreamCreated { get; set; }

  [JsonProperty("background")] public string Background { get; set; }

  [JsonProperty("moderated_background")] public bool ModeratedBackground { get; set; }

  [JsonProperty("moderated_playlist")] public bool ModeratedPlaylist { get; set; }

  [JsonProperty("bg_color")] public string BackgroundColor { get; set; }

  [JsonProperty("bg_opacity")] public double BackgroundOpacity { get; set; }

  [JsonProperty("moderated_item")] public bool ModeratedItem { get; set; }

  [JsonProperty("theme_bg")] public string ThemeBackground { get; set; }

  [JsonProperty("playlist_id")] public int PlaylistId { get; set; }

  [JsonProperty("members_only")] public bool MembersOnly { get; set; }

  [JsonProperty("moderated_suggestions")]
  public bool ModeratedSuggestions { get; set; }

  [JsonProperty("moderated_chat")] public bool ModeratedChat { get; set; }

  [JsonProperty("moderated_user")] public bool ModeratedUser { get; set; }

  [JsonProperty("moderated_cam")] public bool ModeratedCam { get; set; }
}
