using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for discord client service
public interface IDiscordClientService
{
  void Initialize();

  /// <summary>
  /// Logs in the Discord bot using the provided Discord token.
  /// </summary>
  /// <param name="discordToken">The token used to authenticate the bot with the Discord platform.</param>
  /// <returns>Returns a Task representing the asynchronous operation.</returns>
  Task LoginDiscordBotAsync(string discordToken);

  /// <summary>
  ///  Starts the Discord bot, sends READY event to Discord and starts the heartbeat.
  /// </summary>
  /// <returns>Returns a Task representing the asynchronous operation.</returns>
  Task StartDiscordBotAsync();
}
