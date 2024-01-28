using Discord;

namespace Howbot.Core.Models;

public abstract record Constants
{
  public static readonly Color ThemeColor = Color.DarkPurple;

  // Discord Development Server (DevTest2)
  public const ulong DiscordDevelopmentGuildId = 656305202185633810;
  public const string BotName = "Howbot";
  public const string YouTubeBaseShortUrl = "https://youtu.be/";

  public const string YouTubeBaseLongUrl = "https://www.youtube.com/watch?v=";

  // TODO: These will be used for creating API using Discord REST client
  public const string DiscordOAuth2BaseUrl = "https://discord.com/oauth2/authorize";
  public const string DiscordTokenBaseUrl = "https://discord.com/api/oauth2/token";
  public const string DiscordTokenRevokeBaseUrl = "https://discord.com/api/oauth2/token/revoke";
  public const int MaximumUniqueSearchAttempts = 5;
  public const int RelatedSearchResultsLimit = 3;
  public const int ApplicationTimeoutInMs = 3000;
  public const int MaximumMessageCount = 10000;
  public const string DefaultPrefix = "!~";

  public readonly struct Commands
  {
    // Join Command
    public const string JoinCommandName = "join";

    public const string JoinCommandDescription = "Join a valid server voice channel, otherwise nothing.";

    // Play Command
    public const string PlayCommandName = "play";

    public const string PlayCommandDescription =
      "Plays a track from a given search query. Optionally, can choose different search providers.";

    // Play Command Args
    public const string PlaySearchRequestArgumentName = "search_request";
    public const string PlaySearchRequestArgumentDescription = "Search request used for search provider.";
    public const string PlaySearchTypeArgumentName = "search_type";

    public const string PlaySearchTypeArgumentDescription =
      "Optional search provider for finding audio response with provided search request.";

    // Pause Command
    public const string PauseCommandName = "pause";

    public const string PauseCommandDescription = "Pauses a currently playing song, otherwise nothing.";

    // Resume Command
    public const string ResumeCommandName = "resume";

    public const string ResumeCommandDescription = "Resumes the currently paused or stopped track, otherwise nothing.";

    // Seek Command
    public const string SeekCommandName = "seek";

    public const string SeekCommandDescription = "Seek to a position on the current playing track.";

    // Skip Command
    public const string SkipCommandName = "skip";

    public const string SkipCommandDescription =
      "Skip either to the next track or a valid number of songs in the track queue.";

    // Volume Command
    public const string VolumeCommandName = "volume";

    public const string VolumeCommandDescription = "Change the volume of the current playing track.";

    // NowPlaying Command
    public const string NowPlayingCommandName = "nowplaying";

    public const string NowPlayingCommandDescription = "Gets an embed for the current playing/paused track.";

    // Genius Command
    public const string GeniusCommandName = "genius";

    public const string GeniusCommandDescription = "Gets lyrics for the current playing track using Genius lyrics.";

    // Ovh Command
    public const string OvhCommandName = "ovh";

    public const string OvhCommandDescription = "Gets lyrics for the current playing track using Ovh lyrics.";

    // Leave Command
    public const string LeaveCommandName = "leave";

    public const string LeaveCommandDescription =
      "Leaves the current voice channel and removes any songs from the queue.";

    // Ping Command
    public const string PingCommandName = "ping";

    public const string PingCommandDescription = "Ping the Discord websocket API.";

    // Help Command
    public const string HelpCommandName = "help";

    public const string HelpCommandDescription = "See all available commands for the bot.";

    // Radio Command
    public const string RadioCommandName = "radio";

    public const string RadioCommandDescription = "Plays a radio station by a given genre or radio station name.";

    // Shuffle Command
    public const string ShuffleCommandName = "shuffle";

    public const string ShuffleCommandDescription = "Shuffles the current servers music queue.";

    // 24/7 Command
    public const string TwoFourSevenCommandName = "247";

    public const string TwoFourSevenCommandDescription =
      "Toggles 24/7 for playing music based off related.";

    // Lyrics Command
    public const string LyricsCommandName = "lyrics";
    public const string LyricsCommandDescription = "Gets lyrics from lyrics.ovh for the current playing track.";

    #region Admin Commands

    // Purge Command
    public const string PurgeCommandName = "purge";
    public const string PurgeCommandDescription = "Purge current text channel from up to 10,000 messages.";

    public const string BanCommandName = "ban";
    public const string BanCommandDescription = "Permanently ban a user from the server.";

    #endregion
  }
}
