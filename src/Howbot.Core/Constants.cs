namespace Howbot.Core;

public abstract record Constants
{
  // Discord Development Server (DevTest2)
  public const ulong DiscordDevelopmentGuildId = 656305202185633810;

  public struct Commands
  {
    // Join Command
    public const string JoinCommandName = "join";
    public const string JoinCommandDescription = "Join a valid Discord voice channel, otherwise nothing.";
    
    // Play Command
    public const string PlayCommandName = "play";
    public const string PlayCommandDescription =
      "Plays a track from a given search query. Optionally, can choose different search providers.";
    // Play Command Args
    public const string PlaySearchRequestArgumentName = "search_request";
    public const string PlaySearchRequestArgumentDescription = "Search request used for search provider.";
    public const string PlaySearchTypeArgumentName = "search_type";
    public const string PlaySearchTypeArgumentDescription = "Optional search provider for finding audio response with provided search request.";

    // Pause Command
    public const string PauseCommandName = "pause";
    public const string PauseCommandDescription = "Pauses a currently playing song, otherwise nothing.";
    
    // Resume Command
    public const string ResumeCommandName = "resume";
    public const string ResumeCommandDescription = "Resumes the currently paused or stopped track, otherwise nothing.";
  }
}
