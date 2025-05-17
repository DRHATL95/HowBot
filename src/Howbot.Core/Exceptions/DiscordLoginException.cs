namespace Howbot.Core.Exceptions;

public class DiscordLoginException : Exception
{
  public DiscordLoginException(string message) : base(message)
  {
  }
}
