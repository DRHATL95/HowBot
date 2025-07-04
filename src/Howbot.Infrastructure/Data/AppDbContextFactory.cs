using Howbot.SharedKernel.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Howbot.Infrastructure.Data;

public class AppDbContextFactory() : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    // Build config to get connection string
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // Needed so EF CLI finds appsettings
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var connectionString = Environment.GetEnvironmentVariable("PostgresConnectionString") ?? throw new InvalidOperationException("Connection string not found.");

    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    
    return new AppDbContext(optionsBuilder.Options);
  }
}
