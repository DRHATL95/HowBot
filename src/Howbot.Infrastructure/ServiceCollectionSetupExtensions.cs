using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Howbot.Infrastructure.Data;
using Howbot.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure;

public static class ServiceCollectionSetupExtensions
{
  public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
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
    services.AddSingleton<ILavaNodeService, LavaNodeService>();
    services.AddSingleton<IMusicService, MusicService>();
    services.AddSingleton<IEmbedService, EmbedService>();
    services.AddSingleton<IDiscordClientService, DiscordClientService>();
    services.AddSingleton<IInteractionHandlerService, InteractionHandlerService>();
    services.AddSingleton<IDockerService, DockerService>();
    services.AddSingleton<IDeploymentService, DeploymentService>();

    services.AddTransient<IHttpService, HttpService>();
  }
}
