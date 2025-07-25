using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Settings;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Services;
using Serilog;
using Serilog.Events;

namespace Howbot.Worker;

public static class Program
{
  /// <summary>
  ///   The main entry point for the application.
  /// </summary>
  /// <param name="args">The command-line arguments.</param>
  /// <returns>The exit code that is given to the operating system after the app ends.</returns>
  private static async Task<int> Main(string[] args)
  {
    try
    {
      // Create host builder that will be used to handle application (console) life-cycle.
      var hostBuilder = CreateHostBuilder(args);

      Log.Logger.Information("Starting worker application...");

      // Will run indefinitely until canceled w/ cancellation token or process is stopped.
      await hostBuilder.RunConsoleAsync();
    }
    catch (Exception exception)
    {
      Log.Logger.Fatal(exception, nameof(Main));

      if (Log.IsEnabled(LogEventLevel.Fatal))
      {
        Log.Fatal(exception, "A fatal exception has been thrown while running the application");
      }
      else
      {
        Console.WriteLine(exception);
      }
    }

    // Return exit code to terminal once application has been terminated.
    return Environment.ExitCode;
  }


  /// <summary>
  ///   Creates a host builder that configures the services for the application.
  /// </summary>
  /// <param name="args">The command-line arguments.</param>
  /// <returns>A configured IHostBuilder.</returns>
  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .UseSerilog((context, configuration) =>
      {
        context.Configuration["ConnectionStrings:DefaultConnection"] = Configuration.PostgresConnectionString;
        configuration
          .ReadFrom.Configuration(context.Configuration);
      })
      .ConfigureServices((hostContext, services) =>
      {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IServiceLocator, ServiceScopeFactoryLocator>();

        services.AddSingleton<INotificationChannel, InMemoryNotificationChannel>();
        services.AddSingleton<INotificationService, HybridNotificationService>();

        services.AddHostedService<NotificationBackgroundService>();

        services.AddHowbotServices();
        services.AddLavalinkServices();

        // Add in-memory cache
        services.AddMemoryCache();

        // Add static host configuration for access globally
        ConfigurationHelper.SetHostConfiguration(hostContext.Configuration);

        // Infrastructure.ContainerSetup
        if (ConfigurationHelper.HostConfiguration is not null)
        {
          services.AddDbContext(ConfigurationHelper.HostConfiguration);
        }

        services.AddRepositories();

        var workerSettings = new WorkerSettings();
        hostContext.Configuration.Bind(nameof(WorkerSettings), workerSettings);
        services.AddSingleton(workerSettings);

        services.AddHostedService<Worker>();
      });
  }
}
