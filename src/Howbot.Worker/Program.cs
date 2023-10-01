using System;
using System.Diagnostics;
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
using Lavalink4NET.InactivityTracking.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Howbot.Worker;

[UsedImplicitly]
public class Program
{
  static async Task<int> Main(string[] args)
  {
    try
    {
      // Create host builder that will be used to handle application (console) life-cycle.
      var hostBuilder = CreateHostBuilder(args);

      // Will run indefinitely until canceled w/ cancellation token or process is stopped.
      await hostBuilder.RunConsoleAsync().ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      if (Log.IsEnabled(LogEventLevel.Fatal))
      {
        Log.Fatal(exception, "An exception has been thrown while running the application.");
      }

      Debug.Write("An exception has caused the application to close");
      return Environment.ExitCode;
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
        services.AddSingleton(x =>
          new InteractionService(x.GetRequiredService<DiscordSocketClient>(), Configuration.InteractionServiceConfig));
        services.AddSingleton(x => new YouTubeService(new BaseClientService.Initializer
        {
          ApiKey = Configuration.YouTubeToken, ApplicationName = Constants.BotName
        }));
        services.AddLavalink();
        services.ConfigureLavalink(x =>
        {
          x.Passphrase = Configuration.AudioServiceOptions.Passphrase;
        });
        services.AddInactivityTracking();

        ConfigurationHelper.SetHostConfiguration(hostContext.Configuration);

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
