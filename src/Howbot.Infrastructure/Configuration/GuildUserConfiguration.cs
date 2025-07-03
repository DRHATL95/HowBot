using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Configuration;
public class GuildUserConfiguration : IEntityTypeConfiguration<GuildUser>
{
  public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<GuildUser> builder)
  {
    builder.HasKey(gu => new { gu.GuildId, gu.UserId });
    builder.Property(gu => gu.GuildId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(gu => gu.UserId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(gu => gu.Username)
        .IsRequired()
        .HasMaxLength(100);
    builder.Property(gu => gu.Discriminator)
        .IsRequired()
        .HasMaxLength(10);

    // Relationships
    builder.HasOne<Guild>()
            .WithMany(g => g.GuildUsers)
            .HasForeignKey(gu => gu.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
  }
}
