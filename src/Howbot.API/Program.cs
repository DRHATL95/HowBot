using Discord.Rest;
using Discord.WebSocket;
using Lavalink4NET.Rest;
using Howbot.Core.Interfaces;
using Howbot.Infrastructure;
using Howbot.Infrastructure.Services;
using Lavalink4NET.Clients;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using SpotifyAPI.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Howbot Services
builder.Services.AddSingleton<DiscordRestClient>();
builder.Services.AddSingleton<LavalinkApiClient>();
builder.Services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
  "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/guilds/{guildId}/music/play", async (string guildId) =>
  {
    if (!ulong.TryParse(guildId, out ulong guildIdParsed))
    {
      return "Invalid guildId";
    }
    
    var client = app.Services.GetRequiredService<DiscordRestClient>();

    var guild = await client.GetGuildAsync(guildIdParsed);
    
    return $"Player with guildId: {guild.Name}";
  })
  .WithName("GetPlayer")
  .WithOpenApi();

app.MapGet("/weatherforecast", () =>
  {
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
          DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
          Random.Shared.Next(-20, 55),
          summaries[Random.Shared.Next(summaries.Length)]
        ))
      .ToArray();
    return forecast;
  })
  .WithName("GetWeatherForecast")
  .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
