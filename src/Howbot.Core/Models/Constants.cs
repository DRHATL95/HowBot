using Discord;

namespace Howbot.Core.Models;

public abstract record Constants
{
  public const string DatabaseConnectionStringName = "DefaultConnection";
  // public const int ApplicationTimeoutInMs = 3000;
  public const int MaximumMessageCount = 10000;
  public const string DefaultPrefix = "!~";
  
  public static readonly Color ThemeColor = Color.DarkPurple;

  public static class Discord
  {
    // Discord Development Server (DevTest2)
    public const ulong DiscordDevelopmentGuildId = 656305202185633810;
    public const string BotName = "Howbot"; 
  }

  /*public static class YouTube
  {
    public const string YouTubeBaseShortUrl = "https://youtu.be/";
    public const string YouTubeBaseLongUrl = "https://www.youtube.com/watch?v="; 
  }*/

  public static class RegexPatterns
  {
    public const string UrlPattern = @"^(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$"; 
    public const string PlaylistRegexPattern = @"playlist\s\w+";
    public const string YoutubePlaylistRegexPattern = @"^(?:http(?:s)?:\/\/)?(?:www\.)?(?:music\.)?youtube\.com\/playlist\?list=(?<id>[a-zA-Z0-9-_]+)$";
    public const string ApplicationIdRegexPattern = @"\|.*\|.*\|.*\|.*\|\s*\n([\s\S]+?)(?=\|\s*\n\|\s*[\-]+\s*\|)";
  }
  
  public struct Commands
  {
    public const string JoinCommandName = "join";
    public const string JoinCommandDescription = "Join a valid server voice channel, otherwise nothing.";

    public const string PlayCommandName = "play";
    public const string PlayCommandDescription = "Plays a track from a given search query. Optionally, can choose different search providers.";

    public const string PlaySearchRequestArgumentName = "search_request";
    public const string PlaySearchRequestArgumentDescription = "Search request used for search provider.";
    public const string PlaySearchTypeArgumentName = "search_type";

    public const string PlaySearchTypeArgumentDescription = "Optional search type used for search request. (Default: YouTube)";

    public const string PauseCommandName = "pause";
    public const string PauseCommandDescription = "Pauses a currently playing song, otherwise nothing.";

    public const string ResumeCommandName = "resume";
    public const string ResumeCommandDescription = "Resumes the currently paused or stopped track, otherwise nothing.";

    public const string SeekCommandName = "seek";
    public const string SeekCommandDescription = "Seek to a position on the current playing track.";

    public const string SkipCommandName = "skip";
    public const string SkipCommandDescription = "Skip either to the next track or a valid number of songs in the track queue.";

    public const string VolumeCommandName = "volume";
    public const string VolumeCommandDescription = "Change the volume of the current playing track. Leaving blank will show the current volume.";

    public const string NowPlayingCommandName = "nowplaying";
    public const string NowPlayingCommandDescription = "Gets an embed for the current playing/paused track.";

    /*public const string GeniusCommandName = "genius";
    public const string GeniusCommandDescription = "Gets lyrics for the current playing track using Genius lyrics.";*/

    /*public const string OvhCommandName = "ovh";
    public const string OvhCommandDescription = "Gets lyrics for the current playing track using Ovh lyrics.";*/

    public const string LeaveCommandName = "leave";
    public const string LeaveCommandDescription = "Leaves the current voice channel and removes any songs from the queue.";

    public const string PingCommandName = "ping";
    public const string PingCommandDescription = "Ping the Discord websocket API.";

    public const string HelpCommandName = "help";
    public const string HelpCommandDescription = "See all available commands for the bot.";

    public const string RadioCommandName = "radio";
    public const string RadioCommandDescription = "Plays a radio station by a given genre or radio station name.";

    public const string ShuffleCommandName = "shuffle";
    public const string ShuffleCommandDescription = "Shuffles the current servers music queue.";

    public const string TwoFourSevenCommandName = "247";
    public const string TwoFourSevenCommandDescription = "Toggles 24/7 for playing music based off related.";

    public const string LyricsCommandName = "lyrics";
    public const string LyricsCommandDescription = "Gets lyrics from lyrics.ovh for the current playing track.";

    public const string QueueCommandName = "queue";
    public const string QueueCommandDescription = "Gets the current music queue for the server.";

    public const string PurgeCommandName = "purge";
    public const string PurgeCommandDescription = "Purge current text channel from up to 10,000 messages.";

    public const string BanCommandName = "ban";
    public const string BanCommandDescription = "Permanently ban a user from the server.";
    
    public const string UnbanCommandName = "unban";
    public const string UnbanCommandDescription = "Unban a user from the server.";

    public const string RollCommandName = "roll";
    public const string RollCommandDescription = "Rolls a dice.";

    public const string FlipCommandName = "flip";
    public const string FlipCommandDescription = "Flips a coin.";
  }
}
