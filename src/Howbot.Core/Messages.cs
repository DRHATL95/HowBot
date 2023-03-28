namespace Howbot.Core;

public abstract record Messages
{
  public readonly struct Responses
  {
    public const string BotNotConnectedToVoiceResponseMessage = "I am not connected to a voice channel";
    public const string BotSkipQueueOutOfBounds = "There are not that many songs in queue. Try a smaller number.";
    public const string BotTrackPaused = "Current track has been paused.";
    public const string RadioModeEnabled = "Radio mode enabled.";
    public const string RadioModeDisabled = "Radio mode disabled";
    public const string UserVoiceConnectionRequired = "You must be in a voice channel to use this command.";
    public const string GeneralCommandFailed = "Command did not run successfully.";
    public const string NoPlayerInVoiceChannelResponse = "There is no player for this voice channel.";
  }

  public readonly struct Debug
  {
    public const string SkipNextTrack = "Skipping to next track.";
    public const string NowPlaying = "Now playing new track";

    public const string ClientNotConnectedToVoiceChannel =
      "Client is not connected to voice channel. Unable to execute command.";

    public const string ClientQueueOutOfBounds = "Requested number of tracks to skip exceeds queue count";
  }

  public readonly struct Errors
  {
    public const string InteractionUnknownCommand = "Unknown command used. Please try another command!";
    public const string InteractionUnknownCommandLog = "Unknown command error thrown at interaction";

    // Similar, but should not get this error message often due to method type checking try/catch
    public const string InteractionConvertFailed = "Unable to convert parameters for command arguments";
    public const string InteractionConvertFailedLog = "Unable to convert command parameter arguments";

    public const string InteractionBadArguments = "Wrong number of arguments provided for this command";
    public const string InteractionBadArgumentsLog = "Wrong number of arguments provided for command";

    public const string InteractionException = "An error has occured trying to run this command";

    public const string InteractionExceptionLog =
      "An exception has been thrown trying to execute an interaction command";

    public const string InteractionUnsuccessful = "Command did not run successfully";
    public const string InteractionUnsuccessfulLog = "Interaction command did not run successfully";

    public const string InteractionUnmetPrecondition = "You do not have permission to run this command";
    public const string InteractionUnmetPreconditionLog = "Unmet precondition trying to run interaction command";

    public const string InteractionParseFailed = "Unable to parse command arguments";
    public const string InteractionParseFailedLog = "Unable to parse command trying to run interaction command";

    public const string InteractionNull = "Something weird happened when trying to run command. Try again";
    public const string InteractionNullLog = "Result error is null when trying to run interaction command";
  }
}
