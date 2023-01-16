namespace Howbot.Core;

public abstract record Messages
{
  // Error Messages
  public abstract record Errors
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
