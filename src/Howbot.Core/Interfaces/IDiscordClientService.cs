using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Howbot.Core.Interfaces;

public interface IDiscordClientService
{

  void Initialize();

  ValueTask<bool> LoginDiscordBotAsync([NotNull] string discordToken);

  ValueTask<bool> StartDiscordBotAsync();

}
