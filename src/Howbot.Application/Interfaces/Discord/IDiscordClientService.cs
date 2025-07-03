namespace Howbot.Application.Interfaces.Discord;

public interface IDiscordClientService
{
  void Initialize();

  Task LoginDiscordBotAsync(string discordToken);

  Task StartDiscordBotAsync();
}
