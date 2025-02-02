using System.Reflection;
using Howbot.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Guild> Guilds { get; set; }
  public DbSet<LavalinkSession> LavalinkSessions { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
