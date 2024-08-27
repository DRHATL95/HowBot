using System.Text.RegularExpressions;

namespace Howbot.Infrastructure.Data.Config;

public static class Constants
{
  public const string EncryptionKey = "Howbot123";

  public const string WatchTogetherCreateRoomUrl = "https://api.w2g.tv/rooms/create.json";
  public const string WatchTogetherRoomUrl = "https://w2g.tv/rooms";
  public const int DefaultUriLength = 1024;

  public const string DogApiUrl = "https://api.thedogapi.com/v1";
  public const string CapApiUrl = "https://api.thecatapi.com/v1";
  public const string EftApiBaseUrl = "https://api.tarkov.dev/graphql";
  
  // This is for application ids, this will provide one line at a time from the markdown file
  // https://raw.githubusercontent.com/Delitefully/DiscordLists/master/activities.md
  public static Regex DiscordApplicationIdsLineRegex { get; } =
    new(@"\|\s*!\[Icon\]\((.*?)\)\s*\|\s*(\d{18})\s*\|\s*([^|]+?)\s*\|\s*([^|]+?)\s*\|");
}
