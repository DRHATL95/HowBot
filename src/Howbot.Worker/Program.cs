using Howbot.Core.Interfaces;
using Howbot.Core.Settings;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Services;
using Serilog;
using Serilog.Events;

namespace Howbot.Worker;

public static class Program
{
  private static async Task<int> Main(string[] args)
  {
    try
    {
      var hostBuilder = CreateHostBuilder(args);

      Log.Logger.Information("Starting worker application...");

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

    return Environment.ExitCode;
  }


  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .UseSerilog((hostContext, loggingConfiguration) =>
      {
        loggingConfiguration
          .ReadFrom.Configuration(hostContext.Configuration);
      })
      .ConfigureServices((hostContext, services) =>
      {
        services.Configure<BotSettings>(hostContext.Configuration);

        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IServiceLocator, ServiceScopeFactoryLocator>();

        services.AddHowbotServices();

        services.AddMemoryCache();

        services.AddRepositories();

        var workerSettings = new WorkerSettings();
        hostContext.Configuration.Bind(nameof(WorkerSettings), workerSettings);

        services.AddSingleton(workerSettings);

        services.AddHostedService<Worker>();
      });
  }
}
