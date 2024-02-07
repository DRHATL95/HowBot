using System;

namespace Howbot.Core.Models.Exceptions;

public class DiscordLoginException : Exception
{
  public DiscordLoginException(string message) : base(message)
  {
  }
}
