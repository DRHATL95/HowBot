namespace Howbot.Core.Interfaces;

public interface IDiscordClientService
{
  void Initialize();

  Task LoginDiscordBotAsync(string discordToken);

  Task StartDiscordBotAsync();
}
