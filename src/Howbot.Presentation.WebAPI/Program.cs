using Howbot.Core.Settings;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Logging
builder.Services.AddLogging(logging =>
{
  logging.ClearProviders();
  logging.AddConsole();
});

// Register HttpClientFactory
builder.Services.AddHttpClient();

// Add Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ILavalinkApiClient>(sp =>
{
  var cache = sp.GetRequiredService<IMemoryCache>();
  var logger = sp.GetRequiredService<ILogger<LavalinkApiClient>>();
  var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
  var options = new LavalinkApiClientOptions
  {
    BaseAddress = Configuration.LavalinkUri,
    Passphrase = Configuration.AudioServiceOptions.Passphrase,
    Label = Configuration.AudioServiceOptions.Label,
    HttpClientName = Configuration.AudioServiceOptions.HttpClientName
  };

  return new LavalinkApiClient(httpClientFactory, (IOptions<LavalinkApiClientOptions>)options, cache, logger);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
