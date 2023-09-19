using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Howbot.Core.Models;

public abstract class RequestCancellationBase
{
  public abstract CancellationToken Token { get; }

  public static implicit operator CancellationToken(RequestCancellationBase requestCancellationBase) => requestCancellationBase.Token;
}

public class RequestCancellation : RequestCancellationBase
{
  public override CancellationToken Token { get; }
}
