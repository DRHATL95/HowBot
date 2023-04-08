using Discord;

namespace Howbot.Core.Models;

public abstract record Permissions
{
  public struct Bot
  {
    #region Guild Permissions

    private const GuildPermission GuildBaseBotPermission = GuildPermission.ViewChannel;

    public const GuildPermission GuildBotVoiceCommandPermission =
      GuildBaseBotPermission | GuildPermission.Connect;

    public const GuildPermission GuildBotVoicePlayCommandPermission =
      GuildBotVoiceCommandPermission | GuildPermission.Speak;

    #endregion

    #region Non-Guild Permissions

    // TODO: dhoward

    #endregion
  }

  public struct User
  {
    #region Guild Permissions

    private const GuildPermission GuildBaseUserApplicationCommandPermission =
      GuildPermission.ViewChannel | GuildPermission.UseApplicationCommands;

    public const GuildPermission GuildUserVoiceCommandPermission =
      GuildBaseUserApplicationCommandPermission | GuildPermission.Connect;

    public const GuildPermission GuildUserVoicePlayCommandPermission =
      GuildUserVoiceCommandPermission | GuildPermission.Speak;

    #endregion

    #region Non-Guild Permissions

    // TODO: dhoward

    #endregion
  }
}
