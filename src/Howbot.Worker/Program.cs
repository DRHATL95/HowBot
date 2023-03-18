using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Docker.DotNet;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Howbot.Core;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Howbot.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Howbot.Core.Settings;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Worker;

public abstract class Program
{
  public static async Task<int> Main(string[] args)
  {
    // Create host builder that will be used to handle application (console) life-cycle.
    var hostBuilder = CreateHostBuilder(args);

    // Will run indefinitely until canceled w/ cancellation token or process is stopped.
    await hostBuilder.RunConsoleAsync();

    // Return exit code to terminal once application has been terminated.
    return Environment.ExitCode;
  }

  /// <summary>
  /// Create the main host builder used to host the service
  /// </summary>
  /// <param name="args">The arguments provided when running</param>
  /// <returns></returns>
  private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureLogging((_, builder) =>
          {
            builder.ClearProviders();
            
            builder.AddSimpleConsole(options =>
            {
              options.IncludeScopes = true;
              options.SingleLine = true;
              options.TimestampFormat = "[MM/dd/yyyy HH:mm:ss] ";
            });
          })
          .ConfigureServices((hostContext, services) =>
          {
            services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
            services.AddSingleton<IServiceLocator, ServiceScopeFactoryLocator>();
            
            services.AddHowbotServices();
            
            services.AddSingleton<Configuration>();
            services.AddSingleton(x => new DiscordSocketClient(x.GetRequiredService<Configuration>().DiscordSocketConfig));
            services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), x.GetRequiredService<Configuration>().InteractionServiceConfig));
            services.AddSingleton(provider =>
            {
              var discordClient = provider.GetRequiredService<DiscordSocketClient>();
              var nodeConfig = provider.GetRequiredService<Configuration>().NodeConfiguration;
              var logger = provider.GetRequiredService<ILogger<LavaNode<Player<LavaTrack>, LavaTrack>>>();
              return new LavaNode<Player<LavaTrack>, LavaTrack>(discordClient, nodeConfig, logger);
            });
            // services.AddSingleton(x => new DockerClientConfiguration().CreateClient());
            services.AddSingleton(x => new YouTubeService(new BaseClientService.Initializer { ApiKey = x.GetRequiredService<Configuration>().YouTubeToken,  ApplicationName = Constants.BotName }));
            
            // Infrastructure.ContainerSetup
            services.AddDbContext(hostContext.Configuration);
            services.AddRepositories();

            var workerSettings = new WorkerSettings();
            hostContext.Configuration.Bind(nameof(WorkerSettings), workerSettings);
            services.AddSingleton(workerSettings);

            services.AddHostedService<Worker>();
          });
}
