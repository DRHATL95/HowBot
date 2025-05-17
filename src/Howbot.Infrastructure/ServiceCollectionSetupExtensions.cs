using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Settings;
using Howbot.Infrastructure.Data;
using Howbot.Infrastructure.Services;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Lavalink4NET.InactivityTracking.Trackers.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;

namespace Howbot.Infrastructure;

public static class ServiceCollectionSetupExtensions
{
  public static void AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IRepository, EfRepository>();
  }

  public static void AddHowbotServices(this IServiceCollection services)
  {
    services.AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig()
    {
      AlwaysDownloadUsers = true,
      GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.GuildMembers,
      LogLevel = Discord.LogSeverity.Debug,
      LogGatewayIntentWarnings = false,
      UseInteractionSnowflakeDate = false
    }));

    services.AddSingleton(x =>
      new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
      {
        LogLevel = Discord.LogSeverity.Debug,
        DefaultRunMode = RunMode.Async,
        UseCompiledLambda = true
      }));

    services.AddSingleton<IHowbotService, HowbotService>();
    services.AddSingleton<IVoiceService, VoiceService>();
    services.AddSingleton<IMusicService, MusicService>();
    services.AddSingleton<IEmbedService, EmbedService>();
    services.AddSingleton<IDiscordClientService, DiscordClientService>();
    services.AddSingleton<ILavaNodeService, LavaNodeService>();
    services.AddSingleton<IInteractionHandlerService, InteractionHandlerService>();

    var botSettings = services.BuildServiceProvider()
      .GetRequiredService<IOptions<BotSettings>>();

    services.AddDbContext(botSettings.Value);
    services.AddSpotifyService(botSettings.Value);
    services.AddLavalinkServices(botSettings.Value);

    services.AddScoped<IDatabaseService, DatabaseService>();

    services.AddTransient<IHttpService, HttpService>();
  }

  private static void AddDbContext(this IServiceCollection services, BotSettings botSettings)
  {
    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(botSettings.PostgresConnectionString));
  }

  private static void AddSpotifyService(this IServiceCollection services, BotSettings botSettings)
  {
    var spotifyConfig = SpotifyClientConfig.CreateDefault();

    var request = new ClientCredentialsRequest(botSettings.SpotifyClientId, botSettings.SpotifyClientSecret);

    var response = new OAuthClient(spotifyConfig).RequestToken(request).Result;

    services.AddSingleton<ISpotifyClient, SpotifyClient>(_ =>
      new SpotifyClient(spotifyConfig.WithToken(response.AccessToken)));
  }

  private static void AddLavalinkServices(this IServiceCollection serviceCollection, BotSettings botSettings)
  {
    serviceCollection.AddLavalink();

    serviceCollection.ConfigureLavalink(x =>
    {
      x.BaseAddress = new Uri(botSettings.LavalinkNodeAddress);
      x.Passphrase = botSettings.LavalinkNodePassword;
    });

    serviceCollection.AddInactivityTracking();

    serviceCollection.ConfigureInactivityTracking(x =>
    {
      x.DefaultTimeout = TimeSpan.FromSeconds(30);
      x.DefaultPollInterval = TimeSpan.FromSeconds(10);
      x.TrackingMode = InactivityTrackingMode.Any;
      x.InactivityBehavior = PlayerInactivityBehavior.None;
      x.UseDefaultTrackers = true;
      x.TimeoutBehavior = InactivityTrackingTimeoutBehavior.Lowest;
    });

    serviceCollection.Configure<IdleInactivityTrackerOptions>(x =>
    {
      x.Timeout = TimeSpan.FromSeconds(10);
    });

    serviceCollection.Configure<UsersInactivityTrackerOptions>(x =>
    {
      x.Threshold = 1;
      x.Timeout = TimeSpan.FromSeconds(30);
      x.ExcludeBots = true;
    });
  }
}
