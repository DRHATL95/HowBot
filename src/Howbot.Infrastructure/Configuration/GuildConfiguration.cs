using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Constants;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Configuration;
public class GuildConfiguration : IEntityTypeConfiguration<Guild>
{
  public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Guild> builder)
  {
    builder.HasKey(g => g.Id);
    builder.Property(g => g.GuildId)
      .HasConversion<ulong>()
      .HasColumnType("bigint")
      .IsRequired();
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
    builder.Property(g => g.Name)
      .IsRequired()
      .HasMaxLength(100);
    builder.Property(g => g.CreatedAt)
      .IsRequired()
      .HasDefaultValueSql("CURRENT_TIMESTAMP");    

    // Relationships
    builder.HasMany(g => g.GuildUsers)
      .WithOne()
      .HasForeignKey(gu => gu.GuildId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
