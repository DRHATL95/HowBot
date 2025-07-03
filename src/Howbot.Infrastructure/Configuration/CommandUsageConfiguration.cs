using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Configuration;
public class CommandUsageConfiguration : IEntityTypeConfiguration<CommandUsage>
{
  public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CommandUsage> builder)
  {
    builder.HasKey(cu => cu.Id);
    builder.Property(cu => cu.GuildUserId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(cu => cu.GuildId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(cu => cu.CommandName)
        .IsRequired()
        .HasMaxLength(100);
    builder.Property(cu => cu.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");
    builder.Property(cu => cu.IsSuccess)
        .IsRequired()
        .HasDefaultValue(false);

    // Relationships
    builder.HasOne<GuildUser>()
            .WithMany()
            .HasForeignKey(cu => cu.GuildUserId)
            .OnDelete(DeleteBehavior.Cascade);
    builder.HasOne<Guild>()
            .WithMany()
            .HasForeignKey(cu => cu.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
  }
}
