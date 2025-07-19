using System.Configuration;
using Discord.Rest;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Audio.Lavalink;
using Howbot.Infrastructure.Audio.Lavalink.Services;
using Howbot.Infrastructure.Data;
using Howbot.SharedKernel;
using Howbot.SharedKernel.Configuration;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.Configure<BotSettings>(configuration);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => {
  options.UseNpgsql(builder.Configuration["PostgresConnectionString"]);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
builder.Services.AddSingleton<DiscordRestClient>(_ => new DiscordRestClient(new DiscordRestConfig
{
  LogLevel = Discord.LogSeverity.Verbose
}));
builder.Services.AddSingleton<ILavalinkApiClient>(sp =>
{
  var cache = sp.GetRequiredService<IMemoryCache>();
  var logger = sp.GetRequiredService<ILogger<LavalinkApiClient>>();
  var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
  var options = new LavalinkApiClientOptions
  {
    BaseAddress = new Uri(configuration["LavalinkNodeAddress"] ?? string.Empty),
    Passphrase = configuration["LavalinkNodePassword"] ?? string.Empty,
    Label = "howbot-api-client",
    HttpClientName = "howbot-httpclient"
  };

  return new LavalinkApiClient(httpClientFactory, (IOptions<LavalinkApiClientOptions>)options, cache, logger);
});
// builder.Services.AddSingleton<IMusicService, RestMusicService>();
builder.Services.AddSingleton<RestMusicService>();
builder.Services.AddSingleton<ILavalinkSessionProvider, InMemoryLavalinkSessionProvider>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

var botSettings = app.Services.GetRequiredService<IOptions<BotSettings>>();
var discordRestClient = app.Services.GetRequiredService<DiscordRestClient>();
await discordRestClient.LoginAsync(Discord.TokenType.Bot, botSettings.Value.DiscordToken);

discordRestClient.Log += message =>
{
  var logger = app.Services.GetRequiredService<ILoggerAdapter<Program>>();
  logger.LogDebug(message.Message);
  return Task.CompletedTask;
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  // app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
