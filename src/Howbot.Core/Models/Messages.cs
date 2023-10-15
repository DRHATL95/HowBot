namespace Howbot.Core.Models;

public abstract record Messages
{
  public readonly struct Responses
  {
    // Music Bot Responses

    public const string CommandPlayNotSuccessfulResponse =
      "Not able to play the current song request. Try again later.";

    public const string CommandPausedSuccessfulResponse = "Current track has been paused.";

    public const string CommandPausedNotSuccessfulResponse =
      "Current track was not able to be paused. Try again later.";

    public const string CommandResumeSuccessfulResponse = "Current track has been resumed.";

    public const string CommandResumeNotSuccessfulResponse =
      "Current track was not able to be resumed. Try again later.";


    public const string BotNotConnectedToVoiceResponseMessage = "I am not connected to a voice channel";
    public const string BotSkipQueueOutOfBounds = "There are not that many songs in queue. Try a smaller number.";
    public const string BotTrackPaused = "Current track has been paused.";
    public const string BotTrackResumed = "Track has been resumed.";
    public const string BotTrackSkipped = "Skipping to track position in server queue.";
    public const string BotUserVoiceConnectionRequired = "You must be in a voice channel to use this command.";

    public const string BotLeaveVoiceConnection = "Leaving the voice channel.";

    // public const string PlayingRadio = "Now playing radio.";
    public const string BotShuffleQueue = "I have shuffled the music queue currently playing.";
    public const string BotTwoFourSevenOn = "I am turning on 24/7 mode.";
    public const string BotTwoFourSevenOff = "I am turning off 24/7 mode.";
    public const string BotInvalidTimeArgs = "You have provided an invalid time to seek.";
  }

  public readonly struct Debug
  {
    public const string PlayingRadio = "Starting play radio command.";
    public const string SkipNextTrack = "Skipping to next track.";
    public const string NowPlaying = "Now playing new track.";
    public const string Resume = "Resuming current track.";
    public const string Shuffle = "Starting shuffle command.";
    public const string TwoFourSevenOn = "Toggle on 247 command.";
    public const string TwoFourSevenOff = "Toggle off 247 command.";

    public const string ClientNotConnectedToVoiceChannel =
      "Client is not connected to voice channel. Unable to execute command.";

    public const string ClientQueueOutOfBounds = "Requested number of tracks to skip exceeds queue count.";
    public const string DiscordSocketClientConnected = "{Username} has connected to the web socket.";
  }

  public readonly struct Errors
  {
    public const string InteractionUnknownCommand = "Unknown command used. Please try another command!";
    public const string InteractionUnknownCommandLog = "Unknown command error thrown at interaction";
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
    public const string DiscordClientLogin = "Unable to login successfully to discord using provided API token.";
    public const string DiscordStart = "Unable to start discord bot.";
    public const string UnableToGetPlayerForGuild = "Unable to get audio player for Guild VoiceChannel.";
  }
}
