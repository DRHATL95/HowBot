using Howbot.Infrastructure;
using Howbot.Infrastructure.Extensions;
using Howbot.SharedKernel;
using Howbot.SharedKernel.Configuration;
using Howbot.Worker.Discord;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var workerSettings = new WorkerSettings();
builder.Configuration.Bind(workerSettings);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.Configure<BotSettings>(configuration);

builder.Services.AddSerilog((loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext();
});

builder.Services.AddSingleton(workerSettings);
builder.Services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

builder.Services.AddHowbotServices();
builder.Services.AddRepositories();

// builder.Services.AddMemoryCache();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
