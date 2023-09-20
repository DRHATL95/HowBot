using System;

namespace Howbot.Core.Models;
public class DiscordLoginException : Exception
{
  public DiscordLoginException(string message) : base(message)
  {
  }
}
