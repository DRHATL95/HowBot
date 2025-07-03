using Discord;

namespace Howbot.Application.Models.Discord;

public abstract record Permissions
{
  public struct Bot
  {
    private const GuildPermission GuildBaseBotPermission = GuildPermission.ViewChannel;

    public const GuildPermission GuildBotVoiceCommandPermission =
      GuildBaseBotPermission | GuildPermission.Connect;

    public const GuildPermission GuildBotVoicePlayCommandPermission =
      GuildBotVoiceCommandPermission | GuildPermission.Speak;
  }

  public struct User
  {
    private const GuildPermission GuildBaseUserApplicationCommandPermission =
      GuildPermission.ViewChannel | GuildPermission.UseApplicationCommands;

    public const GuildPermission GuildUserVoiceCommandPermission =
      GuildBaseUserApplicationCommandPermission | GuildPermission.Connect;

    public const GuildPermission GuildUserVoicePlayCommandPermission =
      GuildUserVoiceCommandPermission | GuildPermission.Speak;
  }
}
