namespace Howbot.Core.Models.Commands;

public class ApiCommandRequest : CommandRequestBase
{
  public Dictionary<string, string> Arguments { get; init; } = [];

  public static ApiCommandRequest Create(CreateApiCommandRequestParameters parameters)
  {
    return new ApiCommandRequest
    {
      CommandType = parameters.CommandType,
      GuildId = parameters.GuildId,
      Arguments = parameters.Arguments,
      Metadata = new CommandRequestMetadata
      {
        RequestDateTime = DateTime.Now.ToUniversalTime(),
        RequestedById = parameters.UserId,
        Source = CommandSource.Api
      }
    };
  }
}

public struct CreateApiCommandRequestParameters
{
  public CommandTypes CommandType { get; init; }

  public ulong GuildId { get; init; }

  public ulong UserId { get; init; }

  public Dictionary<string, string> Arguments { get; init; }
}
