namespace Howbot.SharedKernel.Configuration;
public class BotSettings
{
  public string DiscordToken { get; set; } = string.Empty;

  public string PostgresConnectionString { get; set; } = string.Empty;

  public string LavalinkNodeAddress { get; set; } = string.Empty;

  public string LavalinkNodePassword { get; set; } = string.Empty;

  public string SpotifyClientId { get; set; } = string.Empty;

  public string SpotifyClientSecret { get; set; } = string.Empty;

  public string WatchTogetherApiKey { get; set; } = string.Empty;
}
