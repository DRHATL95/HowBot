using Howbot.Core.Models.Exceptions;

namespace Howbot.Core.Models.Commands;

public class ApiCommandResponse
{
  public bool IsSuccessful { get; init; }
  public object? Value { get; init; }
  public ApiCommandRequestException? Exception { get; init; }

  public static ApiCommandResponse Create(bool isSuccessful, ApiCommandRequestException? exception = null, object? value = null)
  {
    return new ApiCommandResponse
    {
      IsSuccessful = isSuccessful,
      Exception = exception,
      Value = value
    };
  }
}
