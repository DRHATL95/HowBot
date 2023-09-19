using System.Threading.Tasks;
using JetBrains.Annotations;
using Discord;

namespace Howbot.Core.Interfaces;

public interface IDiscordClientService : IServiceBase
{

  /// <summary>
  /// Calls the <see cref="IDiscordClient"/> login method using the provided auth token.
  /// </summary>
  /// <param name="discordToken"></param>
  /// <returns></returns>
  public ValueTask<bool> LoginDiscordBotAsync([NotNull] string discordToken);

  /// <summary>
  /// Generic method used to handle starting the discord bot. Will be called after <see cref="LoginDiscordBotAsync"/>
  /// </summary>
  /// <returns></returns>
  public ValueTask<bool> StartDiscordBotAsync();
}
