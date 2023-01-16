using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Howbot.Core.Settings;

namespace Howbot.Core.Services;

/// <summary>
/// An example service that performs business logic
/// </summary>
public class EntryPointService : IEntryPointService
{
  private readonly ILoggerAdapter<EntryPointService> _logger;
  private readonly EntryPointSettings _settings;
  private readonly IQueueReceiver _queueReceiver;
  private readonly IQueueSender _queueSender;
  private readonly IServiceLocator _serviceScopeFactoryLocator;
  private readonly IUrlStatusChecker _urlStatusChecker;
  private readonly Configuration _configuration;
  private readonly IInteractionHandlerService _interactionHandlerService;
  private readonly IMusicService _musicService;
  private readonly IEmbedService _embedService;
  private readonly ILavaNodeService _lavaNodeService;
  private readonly IDiscordClientService _discordClientService;

  public EntryPointService(ILoggerAdapter<EntryPointService> logger,
      EntryPointSettings settings,
      IQueueReceiver queueReceiver,
      IQueueSender queueSender,
      IServiceLocator serviceScopeFactoryLocator,
      IUrlStatusChecker urlStatusChecker,
      IDiscordClientService discordClientService,
      IInteractionHandlerService interactionHandlerService,
      IMusicService musicService,
      IEmbedService embedService,
      ILavaNodeService lavaNodeService,
      Configuration configuration)
  {
    _logger = logger;
    _settings = settings;
    _queueReceiver = queueReceiver;
    _queueSender = queueSender;
    _serviceScopeFactoryLocator = serviceScopeFactoryLocator;
    _urlStatusChecker = urlStatusChecker;
    _discordClientService = discordClientService;
    _configuration = configuration;
    _interactionHandlerService = interactionHandlerService;
    _musicService = musicService;
    _embedService = embedService;
    _lavaNodeService = lavaNodeService;
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      if (!await _discordClientService.LoginDiscordBotAsync(_configuration.DiscordToken))
        throw new Exception("Exception thrown logging in to discord API");

      await _discordClientService.StartDiscordBotAsync();

      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

      /*// EF Requires a scope so we are creating one per execution here
      using var scope = _serviceScopeFactoryLocator.CreateScope();
      var repository =
          scope.ServiceProvider
              .GetService<IRepository>();

      // read from the queue
      string message = await _queueReceiver.GetMessageFromQueue(_settings.ReceivingQueueName);
      if (String.IsNullOrEmpty(message)) return;

      // check 1 URL in the message
      var statusHistory = await _urlStatusChecker.CheckUrlAsync(message, "");

      // record HTTP status / response time / maybe existence of keyword in database
      repository.Add(statusHistory);

      _logger.LogInformation(statusHistory.ToString());*/
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, $"{nameof(EntryPointService)}.{nameof(ExecuteAsync)} threw an exception.");
      throw;
    }
  }
}
