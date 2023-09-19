using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Howbot.Infrastructure.Data;
using Howbot.Infrastructure.Http;
using JetBrains.Annotations;
using Lavalink4NET.Extensions;
using Lavalink4NET.Lyrics.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure;

public static class ServiceCollectionSetupExtensions
{
  public static void AddDbContext([NotNull] this IServiceCollection services, [NotNull] IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(([NotNull] options) =>
      options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")));
  }

  public static void AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IRepository, EfRepository>();
  }

  public static void AddHowbotServices(this IServiceCollection services)
  {
    services.AddSingleton<IVoiceService, VoiceService>();
    services.AddSingleton<IMusicService, MusicService>();
    services.AddSingleton<IEmbedService, EmbedService>();
    services.AddSingleton<IDiscordClientService, DiscordClientService>();
    services.AddSingleton<IInteractionHandlerService, InteractionHandlerService>();
    // services.AddSingleton<IDockerService, DockerService>();
    // services.AddSingleton<IDeploymentService, DeploymentService>();
    services.AddSingleton<ILavaNodeService, LavaNodeService>();

    services.AddTransient<IHttpService, HttpService>();
  }
}
