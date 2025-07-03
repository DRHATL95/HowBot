namespace Howbot.SharedKernel.Models;

public abstract class RequestCancellationBase
{
  public abstract CancellationToken Token { get; }

  public static implicit operator CancellationToken(RequestCancellationBase requestCancellationBase)
  {
    return requestCancellationBase.Token;
  }
}

public class RequestCancellation : RequestCancellationBase
{
  public override CancellationToken Token { get; }
}
