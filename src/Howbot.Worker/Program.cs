﻿using System;
using System.Threading.Tasks;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Howbot.Core.Settings;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Howbot.Worker;

public static class Program
{
  private static async Task<int> Main(string[] args)
  {
    try
    {
      // Create host builder that will be used to handle application (console) life-cycle.
      var hostBuilder = CreateHostBuilder(args);

      if (Log.Logger.IsEnabled(LogEventLevel.Information))
      {
        Log.Logger.Information("Starting application..");
      }
      else
      {
        Console.WriteLine("Starting application..");
      }

      // Will run indefinitely until canceled w/ cancellation token or process is stopped.
      await hostBuilder.RunConsoleAsync();
    }
    catch (Exception exception)
    {
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
        
        services.AddHowbotServices();

        // Add in-memory cache
        services.AddMemoryCache();

        // Add static host configuration for access globally
        ConfigurationHelper.SetHostConfiguration(hostContext.Configuration);

        // Infrastructure.ContainerSetup
        services.AddDbContext(ConfigurationHelper.HostConfiguration);
        services.AddRepositories();

        var workerSettings = new WorkerSettings();
        hostContext.Configuration.Bind(nameof(WorkerSettings), workerSettings);
        services.AddSingleton(workerSettings);

        services.AddHostedService<Worker>();
        services.AddHostedService(sp => new MessageQueueConsumerService(Configuration.RabbitMqConnectionFactory, sp.GetRequiredService<IHowbotService>(),
          sp.GetRequiredService<ILoggerAdapter<MessageQueueConsumerService>>()));
      });
  }
}
