namespace Howbot.Core.Models.Commands;

/// <summary>
///   Command requests that come from internally, executed in discord text channels.
/// </summary>
public class CommandRequest : CommandRequestBase
{
  /// <summary>
  ///   The channel where the command was executed from, will be a Discord TextChannel id.
  /// </summary>
  public ulong ChannelId { get; init; }

  /// <summary>
  ///   TODO
  /// </summary>
  /// <param name="commandType">The command request type. Will always be</param>
  /// <param name="guildId">The guild where the command was executed</param>
  /// <param name="channelId">The text channel id where the command was executed</param>
  /// <param name="userId">The user id of the person who submitted the command request</param>
  /// <returns>A new command request</returns>
  public static CommandRequest Create(CommandTypes commandType, ulong guildId, ulong channelId, ulong userId)
  {
    return new CommandRequest
    {
      CommandType = commandType,
      GuildId = guildId,
      ChannelId = channelId,
      Metadata = new CommandRequestMetadata
      {
        Source = CommandSource.Discord, RequestedById = userId, RequestDateTime = DateTime.UtcNow
      }
    };
  }
}
