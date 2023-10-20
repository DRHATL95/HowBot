using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Howbot.Infrastructure.Data;
using Howbot.Infrastructure.Http;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure;

public static class ServiceCollectionSetupExtensions
{
  private const string DatabaseConnectionStringName = "DefaultConnection";

  /// <summary>
  ///   Add the EF DbContext to the service collection.
  ///   Also, configure Npgsql to use the connection string from the appsettings.json file.
  /// </summary>
  /// <param name="services">The service collection where DbContext is added</param>
  /// <param name="configuration">The host builder configuration</param>
  public static void AddDbContext([NotNull] this IServiceCollection services, [NotNull] IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(([NotNull] options) =>
    options.UseNpgsql(
      configuration.GetConnectionString(DatabaseConnectionStringName)));
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
    // Singleton Services
    services.AddSingleton<IVoiceService, VoiceService>();
    services.AddSingleton<IMusicService, MusicService>();
    services.AddSingleton<IEmbedService, EmbedService>();
    services.AddSingleton<IDiscordClientService, DiscordClientService>();
    services.AddSingleton<IInteractionHandlerService, InteractionHandlerService>();
    // services.AddSingleton<IDockerService, DockerService>();
    // services.AddSingleton<IDeploymentService, DeploymentService>();
    services.AddSingleton<ILavaNodeService, LavaNodeService>();
    services.AddScoped<IDatabaseService, DatabaseService>();

    // Transient Services
    services.AddTransient<IHttpService, HttpService>();
  }
}
