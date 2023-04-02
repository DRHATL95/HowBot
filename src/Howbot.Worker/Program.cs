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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Worker;

public abstract class Program
{
  public static async Task<int> Main(string[] args)
  {
    try
    {
      // Create host builder that will be used to handle application (console) life-cycle.
      var hostBuilder = CreateHostBuilder(args);
    
      // Create Serilog instance
      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(Configuration.SerilogConfiguration)
        .CreateLogger();

      // Will run indefinitely until canceled w/ cancellation token or process is stopped.
      await hostBuilder.RunConsoleAsync();

      // Return exit code to terminal once application has been terminated.
      return Environment.ExitCode;
    }
    catch (Exception exception)
    {
      Console.WriteLine(exception);
      throw;
    }
  }

  /// <summary>
  ///   Create the main host builder used to host the service
  /// </summary>
  /// <param name="args">The arguments provided when running</param>
  /// <returns></returns>
  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .ConfigureLogging((_, builder) =>
      {
        builder.ClearProviders();
        builder.AddSerilog();
      })
      .ConfigureServices((hostContext, services) =>
      {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IServiceLocator, ServiceScopeFactoryLocator>();

        services.AddHowbotServices();

        // Add in-memory cache
        services.AddMemoryCache();

        services.AddSingleton<Configuration>();
        services.AddSingleton(x => new DiscordSocketClient(Configuration.DiscordSocketConfig));
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(),
          x.GetRequiredService<Configuration>().InteractionServiceConfig));
        services.AddSingleton(provider =>
        {
          var discordClient = provider.GetRequiredService<DiscordSocketClient>();
          var logger = provider.GetRequiredService<ILogger<LavaNode<Player<LavaTrack>, LavaTrack>>>();
          return new LavaNode<Player<LavaTrack>, LavaTrack>(discordClient, Configuration.NodeConfiguration, logger);
        });
        // services.AddSingleton(x => new DockerClientConfiguration().CreateClient());
        services.AddSingleton(x => new YouTubeService(new BaseClientService.Initializer
        {
          ApiKey = Configuration.YouTubeToken, ApplicationName = Constants.BotName
        }));
        
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
