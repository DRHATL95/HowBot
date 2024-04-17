using System;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Services;
using Howbot.Core.Settings;
using Howbot.Infrastructure.Data;
using Howbot.Infrastructure.Http;
using Howbot.Infrastructure.Services;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Lavalink4NET.InactivityTracking.Trackers.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure;

public static class ServiceCollectionSetupExtensions
{
  /// <summary>
  ///   Add the EF DbContext to the service collection.
  ///   Also, configure Npgsql to use the connection string from the appsettings.json file.
  /// </summary>
  /// <param name="services">The service collection where DbContext is added</param>
  /// <param name="configuration">The host builder configuration</param>
  public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(
        configuration.GetConnectionString(Constants.DatabaseConnectionStringName)));
  }

  /// <summary>
  ///   Add the EfRepository to the service collection used for Entity Framework and Postgres.
  /// </summary>
  /// <param name="services">The service collection where the repository is added.</param>
  public static void AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IRepository, EfRepository>();
  }

  /// <summary>
  ///   Adding the howbot specific services to the service collection.
  ///   Also adds an HttpClient from the Howbot.Infrastructure.Http namespace.
  /// </summary>
  /// <param name="services">The service collection where the services are added.</param>
  public static void AddHowbotServices(this IServiceCollection services)
  {
    // Discord related services
    services.AddSingleton(_ => new DiscordSocketClient(Configuration.DiscordSocketConfig));
    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), Configuration.InteractionServiceConfig));

    // Howbot related services
    services.AddSingleton<IHowbotService, HowbotService>();
    services.AddSingleton<ICommandHandlerService, CommandHandlerService>();
    services.AddSingleton<IVoiceService, VoiceService>(); // TODO: Remove this service, and add functions to music service or lava node service
    services.AddSingleton<IMusicService, MusicService>();
    services.AddSingleton<IEmbedService, EmbedService>();
    services.AddSingleton<IDiscordClientService, DiscordClientService>();
    services.AddSingleton<ILavaNodeService, LavaNodeService>();
    services.AddSingleton<IInteractionHandlerService, InteractionHandlerService>();

    services.AddScoped<IDatabaseService, DatabaseService>();

    // Transient Services
    services.AddTransient<IHttpService, HttpService>();
  }

  public static void AddLavalinkServices(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddLavalink();
    serviceCollection.ConfigureLavalink(x =>
    {
      x.BaseAddress = Configuration.LavalinkUri;
      x.Passphrase = Configuration.AudioServiceOptions.Passphrase;
    });
    serviceCollection.AddInactivityTracking();
    serviceCollection.ConfigureInactivityTracking(x =>
    {
      x.DefaultTimeout = TimeSpan.FromSeconds(30); // default
      x.DefaultPollInterval = TimeSpan.FromSeconds(10); // default is 5 seconds
      x.TrackingMode = InactivityTrackingMode.Any; // default
      x.InactivityBehavior = PlayerInactivityBehavior.None; // default
      x.UseDefaultTrackers = true; // default
      x.TimeoutBehavior = InactivityTrackingTimeoutBehavior.Lowest; // default
    });
    serviceCollection.Configure<IdleInactivityTrackerOptions>(x =>
    {
      x.Timeout = TimeSpan.FromSeconds(10); // default
    });
    serviceCollection.Configure<UsersInactivityTrackerOptions>(x =>
    {
      x.Threshold = 1; // default
      x.Timeout = TimeSpan.FromSeconds(30); // default is 10 seconds
      x.ExcludeBots = true; // default
    });
  }
}
