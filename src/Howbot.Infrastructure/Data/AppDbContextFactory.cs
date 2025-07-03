using Howbot.SharedKernel.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Howbot.Infrastructure.Data;

public class AppDbContextFactory(BotSettings botSettings) : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    
    optionsBuilder.UseNpgsql(botSettings.PostgresConnectionString);
    
    return new AppDbContext(optionsBuilder.Options);
  }
}
