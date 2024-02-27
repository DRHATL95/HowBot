using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Commands;
using Howbot.Core.Models.Exceptions;
using Howbot.Core.Services;
using Howbot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Howbot.Infrastructure.Services;

public class HowbotService(DiscordSocketClient discordSocketClient, IDiscordClientService discordClientService, IMusicService musicService, IServiceProvider serviceProvider, ILoggerAdapter<HowbotService> logger) : ServiceBase<HowbotService>(logger), IHowbotService, IDisposable
{
  // TODO: This will be used to store the session IDs of the guilds that the bot is connected to
  // IMPORTANT: The music functionality requires session IDs to get the player for the guild
  public ConcurrentDictionary<ulong, string> SessionIds { get; set; } = new();

  public async Task StartWorkerServiceAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      
      InitializeHowbotServices(cancellationToken);

      await LoginBotToDiscordAsync(Configuration.DiscordToken, cancellationToken);

      await StartDiscordBotAsync(cancellationToken);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StartWorkerServiceAsync));
      throw;
    }
  }

  public async Task StopWorkerServiceAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await discordSocketClient.StopAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StopWorkerServiceAsync));
      throw;
    }
  }
  
  private async Task LoginBotToDiscordAsync(string discordToken, CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await discordClientService.LoginDiscordBotAsync(discordToken);
    }
    catch (DiscordLoginException discordLoginException)
    {
      Logger.LogError(discordLoginException, nameof(LoginBotToDiscordAsync));
      throw;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(LoginBotToDiscordAsync));
      throw;
    }
  }

  private async Task StartDiscordBotAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await discordClientService.StartDiscordBotAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StartDiscordBotAsync));
      throw;
    }
  }
  
  private void InitializeHowbotServices(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      serviceProvider.GetRequiredService<IHowbotService>()?.Initialize();
      serviceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      serviceProvider.GetRequiredService<ILavaNodeService>()?.Initialize();
      serviceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();
      serviceProvider.GetRequiredService<IEmbedService>()?.Initialize();
      serviceProvider.GetRequiredService<IMusicService>()?.Initialize();
      serviceProvider.GetRequiredService<IInteractionService>()?.Initialize();

      using var scope = serviceProvider.CreateScope();
      scope.ServiceProvider.GetRequiredService<IDatabaseService>()?.Initialize();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(InitializeHowbotServices));
      throw;
    }
  }

  public async Task<CommandResponse> HandleCommandAsync(string commandAsJson)
  {
    try
    {
      Guard.Against.NullOrEmpty(commandAsJson, nameof(commandAsJson));
      
      var command = JsonSerializer.Deserialize<CommandRequest>(commandAsJson);

      switch (command.CommandType)
      {
        case CommandTypes.SendMessage:
          await HandleSendMessageCommandAsync(command);
          break;
        case CommandTypes.SendEmbed:
          break;
        case CommandTypes.JoinVoiceChannel:
          await HandleJoinVoiceChannelCommandAsync(command);
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
        case CommandTypes.Queue:
          var response = await HandleGetQueueCommandAsync(command);
          return response;
        case CommandTypes.Unknown:
        default:
          break;
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleCommandAsync));
      throw; // TODO: Re-evaluate this
    }
    
    return new CommandResponse();
  }

  private async Task<CommandResponse> HandleGetQueueCommandAsync(CommandRequest command)
  {
    try
    {
      Guard.Against.Null(command, nameof(command));

      var guild = discordSocketClient.GetGuild(command.GuildId);
      if (guild is null)
      {
        throw new CommandException("Guild not found.");
      }

      var voiceChannelId = GetVoiceChannel(guild);
      if (voiceChannelId is null)
      {
        throw new CommandException("Bot is not connected to a voice channel in this guild.");
      }

      var player = await musicService.GetPlayerByGuildIdAsync(guild.Id);
      if (player is null)
      {
        throw new CommandException("Player not found.");
      }

      return musicService.GetMusicQueueForServer(player);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleGetQueueCommandAsync));
    }

    return new CommandResponse();
  }

  private async Task HandleSendMessageCommandAsync(CommandRequest command)
  {
    try
    {
      Guard.Against.Null(command, nameof(command));
      
      if (!command.Arguments.TryGetValue("channelId", out var channelIdObj) ||
          !command.Arguments.TryGetValue("message", out var messageObj))
      {
        Console.WriteLine("Missing parameters for SendMessage command.");
        return;
      }
      
      if (channelIdObj is not { } channelId || messageObj is not { } message)
      {
        Console.WriteLine("Invalid parameter types for SendMessage command.");
        return;
      }

      if (discordSocketClient.GetChannel(ulong.Parse(channelId)) is ISocketMessageChannel channel)
      {
        await channel.SendMessageAsync(message);
        Console.WriteLine($"Message sent to channel {channelId}: {message}");
      }
      else
      {
        Console.WriteLine($"Channel {channelId} not found.");
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleSendMessageCommandAsync));
      throw; // TODO: Re-evaluate this
    }
  }

  private async Task HandleJoinVoiceChannelCommandAsync(CommandRequest command)
  {
    try
    {
      Guard.Against.Null(command, nameof(command));

      if (!command.Arguments.TryGetValue("channelId", out var channelIdAsString))
      {
        throw new CommandException("Missing parameters for JoinVoiceChannel command.");
      }

      if (!ulong.TryParse(channelIdAsString, out ulong channelId))
      {
        throw new CommandException("Invalid parameter types for JoinVoiceChannel command.");
      }
      
      var voiceChannel = GetVoiceChannel(channelId);
      
      // Ensure voice channel isn't null
      Guard.Against.Null(voiceChannel, nameof(voiceChannel));
      
      await musicService.JoinVoiceChannelAsync(voiceChannel.GuildId, voiceChannel.Id);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(HandleJoinVoiceChannelCommandAsync));
      throw;
    }
  }

  private IVoiceChannel GetVoiceChannel(ulong channelId)
  {
    return discordSocketClient.GetChannel(channelId) as IVoiceChannel;
  }

  private SocketVoiceChannel GetVoiceChannel(IGuild guild)
  {
    var socketGuild = guild as SocketGuild;
    return socketGuild?.VoiceChannels.FirstOrDefault(x => x.Users.Any(u => u.Id == discordSocketClient.CurrentUser.Id));
  }
  
  public void Dispose()
  {
    GC.SuppressFinalize(this);
    
    discordSocketClient?.Dispose();
  }
}
