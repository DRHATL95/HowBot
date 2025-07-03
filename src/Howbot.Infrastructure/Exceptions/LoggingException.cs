using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Infrastructure.Exceptions;
public class LoggingException : Exception
{
    public LoggingException(string message) : base(message)
    {
    }
    public LoggingException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public LoggingException(Exception innerException) : base("An error occurred while logging.", innerException)
    {
  }
}
