using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Infrastructure.Discord.Exceptions;
public class DiscordLoginException : Exception
{
    public DiscordLoginException()
        : base("An error occurred while logging in to Discord.")
    {
    }
    public DiscordLoginException(string message)
        : base(message)
    {
    }
    public DiscordLoginException(string message, Exception innerException)
        : base(message, innerException)
    {
  }
}
