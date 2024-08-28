using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Commands;
using Howbot.Core.Settings;
using Howbot.Infrastructure.Data.Models.Responses;
using Howbot.Infrastructure.Data.Models.Watch2Gether;
using Newtonsoft.Json;
using Constants = Howbot.Infrastructure.Data.Config.Constants;

namespace Howbot.Infrastructure.Services;

public class HttpService(IHttpClientFactory httpClientFactory) : IHttpService, IDisposable
{
  private readonly HttpClient _client = httpClientFactory.CreateClient("HttpService");

  public void Dispose()
  {
    _client.Dispose();

    GC.SuppressFinalize(this);
  }

  public async Task<int> GetUrlResponseStatusCodeAsync(string url, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var result = await _client.GetAsync(url, cancellationToken);

    return (int)result.StatusCode;
  }

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
      Content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json")
    };

    var httpResponseMessage = await _client.SendAsync(request, cancellationToken);
    httpResponseMessage.EnsureSuccessStatusCode();

    var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
    var convertedResponse = JsonConvert.DeserializeObject<Watch2GetherUrlResponse>(response) ??
                            throw new Exception("Unable to parse response");

    return $"{Constants.WatchTogetherRoomUrl}/{convertedResponse.StreamKey}";
  }

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

      var match = Constants.DiscordApplicationIdsLineRegex.Match(line);
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

  public async Task<string> StartDiscordActivityAsync(string channelId, string activityId,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var requestUri = $"https://discord.com/api/v9/channels/{channelId}/invites";

    // Use Newtonsoft for this one
    var requestContent = new StringContent(JsonConvert.SerializeObject(new
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

    var invite = JsonConvert.DeserializeObject<DiscordInviteResponse>(responseContent) ??
                 throw new Exception($"Failed to start activity. Invite is null. Response: {responseContent}");

    if (string.IsNullOrEmpty(invite.Code))
    {
      throw new Exception($"Failed to start activity. Invite code is null. Response: {responseContent}");
    }

    // Return the invite link
    return $"https://discord.gg/{invite.Code}";
  }

  public async Task<string> GetRandomCatImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (limit is < 1 or > 10)
    {
      throw new Exception("Invalid number of cat images requested. Limit must be between 1 and 10.");
    }

    var url = $"{Constants.CapApiUrl}/images/search?limit={limit}";

    var response = await _client.GetAsync(url, cancellationToken);
    // Limit doesn't work without API key, so anything with limit will return 10
    var responseContent = await response.Content.ReadFromJsonAsync<CatImageResponse[]>(cancellationToken);

    if (responseContent is null || !response.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to get cat image. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    if (limit > 1)
    {
      // Only take the limit amount of images
      // see above comment about limit not working without API key
      responseContent = responseContent.Take(limit).ToArray();
    }

    // Default behavior
    return limit == 1 ? responseContent[0].Url : string.Join(",", responseContent.Select(x => x.Url));
  }

  public async Task<string> GetRandomDogImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (limit is < 1 or > 10)
    {
      throw new Exception("Invalid number of dog images requested. Limit must be between 1 and 10.");
    }

    var url = $"{Constants.DogApiUrl}/images/search?limit={limit}";

    var response = await _client.GetAsync(url, cancellationToken);
    // Limit doesn't work without API key, so anything with limit will return 10
    var responseContent = await response.Content.ReadFromJsonAsync<DogImageResponse[]>(cancellationToken);

    if (responseContent is null || !response.IsSuccessStatusCode)
    {
      throw new Exception($"Failed to get dog image. Status code: {response.StatusCode}. Response: {responseContent}");
    }

    if (limit > 1)
    {
      // Only take the limit amount of images
      // see above comment about limit not working without API key
      responseContent = responseContent.Take(limit).ToArray();
    }

    // Default behavior
    return limit == 1 ? responseContent[0].Url : string.Join(",", responseContent.Select(x => x.Url));
  }

  public async Task<Tuple<string, string, int>?> GetTarkovMarketPriceByItemNameAsync(string itemName,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var requestQuery =
      $"{{items(name: \"{itemName}\") {{id name shortName basePrice wikiLink avg24hPrice iconLink updated sellFor {{price currency priceRUB vendor {{ name normalizedName }}  }} }}}}";
    var data = new Dictionary<string, string> { { "query", requestQuery } };

    var apiResponse = await _client.PostAsJsonAsync(Constants.EftApiBaseUrl, data, cancellationToken);

    apiResponse.EnsureSuccessStatusCode();

    EftMarketResponse? parseResponse;

    try
    {
      var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

      // Parse with Newtonsoft
      parseResponse = JsonConvert.DeserializeObject<EftMarketResponse>(content);
      if (parseResponse is null)
      {
        return null;
      }
    }
    catch (Exception e)
    {
      throw new Exception($"Failed to parse Tarkov API response. {e.Message}");
    }

    if (!parseResponse.Data.Items.Any() || parseResponse.Data.Items.All(x => !x.SellFor.Any()))
    {
      return null;
    }

    var dictionary = new Dictionary<string, int>();

    foreach (var item in parseResponse.Data.Items)
    {
      var splitName = item.Name?.Split(' ');
      if (splitName != null && splitName.Contains(itemName, StringComparer.OrdinalIgnoreCase))
      {
        if (item.Name != null)
        {
          dictionary[item.Name] = 0;
        }
      }
      else
      {
        if (item.Name != null)
        {
          dictionary[item.Name] = StringHelper.CalculateLevenshteinDistance(itemName, item.Name);
        }
      }
    }

    var result = dictionary.OrderBy(kvp => kvp.Value).FirstOrDefault();

    if (result.Key is null)
    {
      return null;
    }

    var marketItem = parseResponse.Data.Items.FirstOrDefault(x => x.Name == result.Key);

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

      maxPrice = item.Price ?? 0;
      trader = item.Vendor.Name;
    }

    if (maxPrice == 0 || string.IsNullOrEmpty(trader))
    {
      return null;
    }

    return new Tuple<string, string, int>(result.Key, trader, maxPrice);
  }

  public async Task<string> GetTarkovTaskByTaskNameAsync(string taskName, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    const string requestQuery = "{tasks(lang: en) {id name taskImageLink}}";
    var data = new Dictionary<string, string> { { "query", requestQuery } };

    var apiResponse = await _client.PostAsJsonAsync(Constants.EftApiBaseUrl, data, cancellationToken);

    apiResponse.EnsureSuccessStatusCode();

    try
    {
      var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

      var parseResponse = JsonConvert.DeserializeObject<EftTaskResponse>(content);

      if (parseResponse is null || !parseResponse.Data.Tasks.Any())
      {
        return string.Empty;
      }

      var task = parseResponse.Data.Tasks.FirstOrDefault(x =>
        x.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));

      return task is null ? string.Empty : task.Name;
    }
    catch (Exception e)
    {
      throw new Exception($"Failed to parse Tarkov API response. {e.Message}");
    }
  }
}
