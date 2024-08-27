using Newtonsoft.Json;

namespace Howbot.Infrastructure.Data.Models.Responses;

public record Watch2GetherUrlResponse
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
