using Discord;
using Discord.Rest;
using Howbot.Core.Interfaces;
using Howbot.Core.Settings;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Services;
using Lavalink4NET.Rest;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
builder.Services.AddSingleton<DiscordRestClient>(_ => new DiscordRestClient(new DiscordRestConfig()
{
  LogLevel = LogSeverity.Info
}));

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.TryAddSingleton<ILavalinkApiClientFactory, LavalinkApiClientFactory>();

// RabbitMQ - Web API only needs to publish messages
builder.Services.AddSingleton<MessageQueuePublisherService>(sp => new MessageQueuePublisherService(Configuration.RabbitMqConnectionFactory, sp.GetRequiredService<ILoggerAdapter<MessageQueuePublisherService>>()));

var app = builder.Build();

var discordRestClient = app.Services.GetRequiredService<DiscordRestClient>();
await discordRestClient.LoginAsync(TokenType.Bot, Configuration.DiscordToken);

discordRestClient.Log += (message) =>
{
  app.Services.GetRequiredService<ILoggerAdapter<Program>>().Log(LogLevel.Information, message.Message);
  return Task.CompletedTask;
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
