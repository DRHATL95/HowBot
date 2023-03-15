namespace Howbot.Core;

public abstract record Constants
{
  // Discord Development Server (DevTest2)
  public const ulong DiscordDevelopmentGuildId = 656305202185633810;
  public const string BotName = "Howbot";

  public readonly struct Commands
  {
    // Join Command
    public const string JoinCommandName = "join";
    public const string JoinCommandDescription = "Join a valid server voice channel, otherwise nothing.";

    #region Play Command

    // Play Command
    public const string PlayCommandName = "play";
    public const string PlayCommandDescription = "Plays a track from a given search query. Optionally, can choose different search providers.";
    
    // Play Command Args
    public const string PlaySearchRequestArgumentName = "search_request";
    public const string PlaySearchRequestArgumentDescription = "Search request used for search provider.";
    public const string PlaySearchTypeArgumentName = "search_type";
    public const string PlaySearchTypeArgumentDescription = "Optional search provider for finding audio response with provided search request.";

    #endregion

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
      "Leaves the current voice channel and removes any songs from the queue";
  }
}
