﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
  /// Gets the response status code of the specified URL.
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
  /// Creates a Watch2Gether room for watching videos together.
  /// </summary>
  /// <param name="url">The URL of the video to be shared.</param>
  /// <returns>The URL of the created Watch2Gether room.</returns>
  public async Task<string> CreateWatchTogetherRoomAsync(string url)
  {
    using var client = new HttpClient();

    Watch2GetherParameters parameters = new Watch2GetherParameters()
    {
      W2GApiKey = Core.Settings.Configuration.WatchTogetherApiKey,
      Share = url,
      BackgroundColor = "#00ff00",
      BackgroundOpacity = "50"
    };

    var convertedParams = JsonConvert.SerializeObject(parameters);
    
    var request = new HttpRequestMessage()
    {
      RequestUri = new Uri(Constants.WatchTogetherCreateRoomUrl),
      Method = HttpMethod.Post,
      Headers =
      {
        Accept =
        {
          new MediaTypeWithQualityHeaderValue("application/json")
        },
      },
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

    string version = string.Empty;

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

  public async Task<string> StartDiscordActivity(string channelId, string activityId)
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
}

struct DiscordInvite
{
  public string Code { get; set; }
}

struct Watch2GetherParameters
{
  [JsonProperty("w2g_api_key")]
  public string W2GApiKey { get; set; }
  
  [JsonProperty("share")]
  public string Share { get; set; }
  
  [JsonProperty("bg_color")]
  public string BackgroundColor { get; set; }
  
  [JsonProperty("bg_opacity")]
  public string BackgroundOpacity { get; set; }
}

struct Watch2GetherResponse
{
  [JsonProperty("id")]
  public int Id { get; set; }
  
  [JsonProperty("streamkey")]
  public string StreamKey { get; set; }
  
  [JsonProperty("created_at")]
  public DateTime CreatedAt { get; set; }
  
  [JsonProperty("persistent")]
  public bool Persistent { get; set; }
  
  [JsonProperty("persistent_name")]
  public string PersistentName { get; set; }
  
  [JsonProperty("deleted")]
  public bool Deleted { get; set; }
  
  [JsonProperty("moderated")]
  public bool Moderated { get; set; }
  
  [JsonProperty("location")]
  public string Location { get; set; }
  
  [JsonProperty("stream_created")]
  public bool StreamCreated { get; set; }
  
  [JsonProperty("background")]
  public string Background { get; set; }
  
  [JsonProperty("moderated_background")]
  public bool ModeratedBackground { get; set; }
  
  [JsonProperty("moderated_playlist")]
  public bool ModeratedPlaylist { get; set; }
  
  [JsonProperty("bg_color")]
  public string BackgroundColor { get; set; }
  
  [JsonProperty("bg_opacity")]
  public double BackgroundOpacity { get; set; }
  
  [JsonProperty("moderated_item")]
  public bool ModeratedItem { get; set; }
  
  [JsonProperty("theme_bg")]
  public string ThemeBackground { get; set; }
  
  [JsonProperty("playlist_id")]
  public int PlaylistId { get; set; }
  
  [JsonProperty("members_only")]
  public bool MembersOnly { get; set; }
  
  [JsonProperty("moderated_suggestions")]
  public bool ModeratedSuggestions { get; set; }
  
  [JsonProperty("moderated_chat")]
  public bool ModeratedChat { get; set; }
  
  [JsonProperty("moderated_user")]
  public bool ModeratedUser { get; set; }
  
  [JsonProperty("moderated_cam")]
  public bool ModeratedCam { get; set; }
}
