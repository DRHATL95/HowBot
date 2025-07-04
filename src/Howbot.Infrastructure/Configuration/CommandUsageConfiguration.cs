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
    builder.ToTable("CommandUsages");

    builder.HasKey(cu => cu.Id);
    builder.Property(cu => cu.Id)
        .HasDefaultValueSql("gen_random_uuid()");

    builder.Property(cu => cu.GuildId).HasColumnType("numeric(20,0)");
    builder.Property(cu => cu.UserId).HasColumnType("numeric(20,0)");

    builder.Property(cu => cu.CommandName)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(cu => cu.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");

    builder.Property(cu => cu.IsSuccess)
        .IsRequired()
        .HasDefaultValue(false);

    builder.HasOne(cu => cu.GuildUser)
        .WithMany()
        .HasForeignKey(cu => new { cu.GuildId, cu.UserId })
        .HasPrincipalKey(gu => new { gu.GuildId, gu.UserId })
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(cu => cu.CreatedAt);
    builder.HasIndex(cu => cu.CommandName);
    builder.HasIndex(cu => new { cu.GuildId, cu.UserId });
  }
}
