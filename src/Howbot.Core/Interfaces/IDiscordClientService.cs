using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IDiscordClientService : IServiceBase
{
  public ValueTask<bool> LoginDiscordBotAsync(string discordToken);
  public ValueTask<bool> StartDiscordBotAsync();
}
