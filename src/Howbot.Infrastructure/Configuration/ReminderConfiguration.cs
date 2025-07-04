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
    builder.ToTable("Reminders");

    builder.HasKey(r => r.Id);
    builder.Property(r => r.Id)
        .HasDefaultValueSql("gen_random_uuid()");

    builder.Property(r => r.GuildId).HasColumnType("numeric(20,0)");
    builder.Property(r => r.UserId).HasColumnType("numeric(20,0)");
    builder.Property(r => r.TextChannelId).HasColumnType("numeric(20,0)");

    builder.Property(r => r.Message)
        .IsRequired()
        .HasMaxLength(500);

    builder.Property(r => r.RemindAt)
        .IsRequired();

    builder.Property(r => r.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");

    builder.HasOne(r => r.GuildUser)
        .WithMany()
        .HasForeignKey(r => new { r.GuildId, r.UserId }) // Composite FK
        .HasPrincipalKey(gu => new { gu.GuildId, gu.UserId }) // Composite PK match
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(r => r.RemindAt);
  }
}
