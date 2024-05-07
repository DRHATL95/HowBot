using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Howbot.Core.Models.Exceptions;
using Newtonsoft.Json;

namespace Howbot.Infrastructure.Services;

public class CommandHandlerService(
  IServiceProvider serviceProvider,
  DiscordSocketClient discordSocketClient,
  InteractionService interactionService,
  IMusicService musicService,
  ILoggerAdapter<CommandHandlerService> logger) : ServiceBase<CommandHandlerService>(logger), ICommandHandlerService
{
  /// <summary>
  ///   This will be called internally when using the bot. This will handle the command request and execute the command.
  /// </summary>
  /// <param name="socketInteractionContext"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task<CommandResponse> HandleCommandRequestAsync(SocketInteractionContext socketInteractionContext,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    Guard.Against.Null(socketInteractionContext, nameof(socketInteractionContext));

    try
    {
      var result = await interactionService.ExecuteCommandAsync(socketInteractionContext, serviceProvider);

      // Due to async nature of InteractionFramework, the result here may always be success.
      // That's why we also need to handle the InteractionExecuted event.
      if (!result.IsSuccess)
      {
        // TODO: Remove logger from this call
        await DiscordHelper.HandleSocketInteractionErrorAsync(socketInteractionContext.Interaction, result, Logger);
      }

      return CommandResponse.Create(result.IsSuccess);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleCommandRequestAsync));

      if (socketInteractionContext.Interaction.Type is InteractionType.ApplicationCommand)
      {
        Logger.LogInformation("Attempting to delete the failed command");

        // If exception is thrown, acknowledgement will still be there. This will clean up.
        await socketInteractionContext.Interaction.DeleteOriginalResponseAsync();

        Logger.LogInformation("Successfully deleted the failed command");
      }
    }

    return CommandResponse.Create(false);
  }

  /// <summary>
  ///   This will be called when using the API. The command will be sent through MQ, parsed and then executed.
  ///   Once executed, the response will be sent back to the API.
  /// </summary>
  /// <param name="commandJson"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task<ApiCommandResponse> HandleCommandRequestAsync(string commandJson,
    CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      Guard.Against.NullOrEmpty(commandJson, nameof(commandJson));

      var command = JsonConvert.DeserializeObject<ApiCommandRequest>(commandJson);
      if (command == null)
      {
        return ApiCommandResponse.Create(false, new ApiCommandRequestException("Invalid command"));
      }

      var response = await HandleCommandExecuteAsync(command, cancellationToken);

      return response;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleCommandRequestAsync));
      throw; // TODO: Re-evaluate this
    }
  }

  private async ValueTask<ApiCommandResponse> HandleCommandExecuteAsync(ApiCommandRequest command,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    ApiCommandResponse? response = null;

    try
    {
      switch (command.CommandType)
      {
        case CommandTypes.SendMessage:
          response = await HandleSendMessageAsync(command);
          break;

        case CommandTypes.SendEmbed:
          break;

        case CommandTypes.JoinVoiceChannel:
          response = await HandleJoinVoiceChannelAsync(command);
          break;

        case CommandTypes.LeaveVoiceChannel:
          break;

        case CommandTypes.Play:
          break;

        case CommandTypes.Stop:
          break;

        case CommandTypes.Skip:
          break;

        case CommandTypes.Pause:
          break;

        case CommandTypes.Resume:
          break;

        case CommandTypes.Guild:
          response = HandleGetGuildById(command);
          break;
        case CommandTypes.Guilds:
          response = HandleGetGuildsForUser(command);
          break;

        case CommandTypes.Queue:

        case CommandTypes.Unknown:
        case CommandTypes.IsPlaying:

        case CommandTypes.Session:

        default:
          throw new ArgumentOutOfRangeException();
      }

      return response ?? ApiCommandResponse.Create(false, new ApiCommandRequestException("Command not handled"));
    }
    catch (Exception exception)
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException(exception.Message));
    }
  }

  #region Api Command Handlers

  private async Task<ApiCommandResponse> HandleSendMessageAsync(ApiCommandRequest command)
  {
    if (!command.Arguments.TryGetValue("message", out var message))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("No message provided for command"));
    }

    if (string.IsNullOrEmpty(message))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Provided message is empty"));
    }

    if (!command.Arguments.TryGetValue("channelId", out var channelId))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("No channel provided to send message to"));
    }

    if (!ulong.TryParse(channelId, out var parsedChannelId))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Invalid channel id provided"));
    }

    var channel = await discordSocketClient.GetChannelAsync(parsedChannelId);

    if (channel is not IMessageChannel messageChannel)
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Channel is not valid"));
    }

    await messageChannel.SendMessageAsync(message)
      .ConfigureAwait(false);

    return ApiCommandResponse.Create(true);
  }

  private async Task<ApiCommandResponse> HandleJoinVoiceChannelAsync(ApiCommandRequest command)
  {
    if (!command.Arguments.TryGetValue("channelId", out var channelId))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("No channel provided to join"));
    }

    if (!ulong.TryParse(channelId, out var parsedChannelId))
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Invalid channel id provided"));
    }

    var channel = discordSocketClient.GetChannel(parsedChannelId);

    if (channel is not IVoiceChannel voiceChannel)
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Channel is not valid"));
    }

    await musicService.JoinVoiceChannelAsync(command.GuildId, voiceChannel.Id);

    return ApiCommandResponse.Create(true);
  }

  private ApiCommandResponse HandleGetGuildById(ApiCommandRequest command)
  {
    var guild = discordSocketClient.GetGuild(command.GuildId);

    if (guild is null)
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("Guild not found"));
    }

    var guildDto = new GuildDto
    {
      Id = guild.Id,
      Name = guild.Name,
      Icon = guild.IconUrl,
      Permissions = (int)guild.CurrentUser.GuildPermissions.RawValue,
      Owner = guild.OwnerId == command.Metadata.RequestedById
    };

    return ApiCommandResponse.Create(true, value: guildDto);
  }

  private ApiCommandResponse HandleGetGuildsForUser(ApiCommandRequest command)
  {
    // Get the guild user 
    var user = discordSocketClient.GetUser(command.Metadata.RequestedById);

    if (user is null)
    {
      return ApiCommandResponse.Create(false, new ApiCommandRequestException("User not found"));
    }

    var guilds = user.MutualGuilds;

    var guildsDto = guilds.Select(g => new GuildDto
    {
      Id = g.Id,
      Name = g.Name,
      Icon = g.IconUrl,
      Permissions = (int)g.CurrentUser.GuildPermissions.RawValue,
      Owner = g.OwnerId == user.Id
    });

    return ApiCommandResponse.Create(true, value: guildsDto);
  }

  #endregion
}
