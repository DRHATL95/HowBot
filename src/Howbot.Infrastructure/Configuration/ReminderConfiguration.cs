using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Configuration;
public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
  public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Reminder> builder)
  {
    builder.HasKey(r => r.Id);
    builder.Property(r => r.GuildUserId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(r => r.TextChannelId)
        .HasConversion<ulong>()
        .HasColumnType("bigint")
        .IsRequired();
    builder.Property(r => r.Message)
        .IsRequired()
        .HasMaxLength(500); // TODO: Create constant for max message length
    builder.Property(r => r.RemindAt)
        .IsRequired();
    builder.Property(r => r.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");
    // Relationships
    builder.HasOne<GuildUser>()
        .WithMany()
        .HasForeignKey(r => r.GuildUserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
