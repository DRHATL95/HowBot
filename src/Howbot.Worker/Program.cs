using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Howbot.Core.Settings;
using Howbot.Infrastructure;
using JetBrains.Annotations;
using Lavalink4NET.Extensions;
using Lavalink4NET.Lyrics.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Howbot.Worker;

[UsedImplicitly]
public class Program
{

  [NotNull] private static readonly ILogger _logger = Log.Logger;

  static async Task<int> Main(string[] args)
  {
    try
    {
      // Create host builder that will be used to handle application (console) life-cycle.
      var hostBuilder = CreateHostBuilder(args);

      // Will run indefinitely until canceled w/ cancellation token or process is stopped.
      await hostBuilder.RunConsoleAsync();
    }
    catch (Exception exception)
    {
      if (Log.IsEnabled(Serilog.Events.LogEventLevel.Error))
      {
        _logger.Error(nameof(Main), exception);
      }
    }

    // Return exit code to terminal once application has been terminated.
    return Environment.ExitCode;
  }

  private static IHostBuilder CreateHostBuilder([NotNull] string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .UseSerilog(([NotNull] context, [NotNull] configuration) =>
      {
        configuration
          .ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext();
      })
      .ConfigureServices(([NotNull] hostContext, [NotNull] services) =>
      {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IServiceLocator, ServiceScopeFactoryLocator>();

        services.AddHowbotServices();

        // Add in-memory cache
        services.AddMemoryCache();

        services.AddSingleton<Configuration>();
        services.AddSingleton(x => new DiscordSocketClient(Configuration.DiscordSocketConfig));
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), Configuration.InteractionServiceConfig));
        // services.AddSingleton(x => new DockerClientConfiguration().CreateClient());
        services.AddSingleton(x => new YouTubeService(new BaseClientService.Initializer
        {
          ApiKey = Configuration.YouTubeToken,
          ApplicationName = Constants.BotName
        }));
        services.AddLavalink();
        services.ConfigureLavalink(x =>
        {
          x.Passphrase = Configuration.AudioServiceOptions.Passphrase;
        });
        services.AddLyrics();

        // Dynamically insert connection string for DB context
        ConfigurationHelper.AddOrUpdateAppSetting("DefaultConnection", Configuration.PostgresConnectionString);

        // Infrastructure.ContainerSetup
        services.AddDbContext(hostContext.Configuration);
        services.AddRepositories();

        var workerSettings = new WorkerSettings();
        hostContext.Configuration.Bind(nameof(WorkerSettings), workerSettings);
        services.AddSingleton(workerSettings);

        services.AddHostedService<Worker>();
      });
  }
}
