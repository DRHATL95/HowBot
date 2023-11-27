using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for discord client service
public interface IDiscordClientService
{
  /// <summary>
  ///   Initialize the discord client service
  /// </summary>
  void Initialize();

  /// <summary>
  ///   Login the discord bot
  /// </summary>
  /// <param name="discordToken">The discord API token</param>
  /// <returns>True, if login was a success</returns>
  ValueTask<bool> LoginDiscordBotAsync(string discordToken);

  /// <summary>
  ///   Calls to start the discord bot
  /// </summary>
  /// <returns>True, if bot was started successfully</returns>
  ValueTask<bool> StartDiscordBotAsync();
}
