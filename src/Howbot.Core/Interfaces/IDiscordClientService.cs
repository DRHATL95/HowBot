using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IDiscordClientService
{
  public ValueTask<bool> LoginDiscordBotAsync(string discordToken);
  public ValueTask<bool> StartDiscordBotAsync();
}
