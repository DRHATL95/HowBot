using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Constants;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howbot.Infrastructure.Configuration;
public class GuildConfiguration : IEntityTypeConfiguration<Guild>
{
  public void Configure(EntityTypeBuilder<Guild> builder)
  {
    builder.ToTable("Guilds");

    builder.HasKey(g => g.Id);
    builder.Property(g => g.Id)
        .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL function for UUIDs

    builder.Property(g => g.GuildId)
        .HasColumnType("numeric(20,0)"); // Maps ulong to numeric for PostgreSQL

    builder.Property(g => g.Name)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(g => g.Prefix)
        .IsRequired()
        .HasMaxLength(10)
        .HasDefaultValue(BotDefaults.DefaultPrefix);

    builder.Property(g => g.Volume)
        .IsRequired()
        .HasDefaultValue(BotDefaults.DefaultVolume);

    builder.Property(g => g.SearchProvider)
        .IsRequired()
        .HasDefaultValue(BotDefaults.DefaultSearchProvider);

    builder.Property(g => g.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");

    builder.HasMany(g => g.GuildUsers)
        .WithOne(gu => gu.Guild)
        .HasForeignKey(gu => gu.GuildId) // FK links to GuildId, not Id
        .HasPrincipalKey(g => g.GuildId) // Important: make it explicit
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(g => g.Name); // For querying by name
  }
}
