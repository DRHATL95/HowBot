using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Domain.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howbot.Infrastructure.Configuration;
public class GuildUserConfiguration : IEntityTypeConfiguration<GuildUser>
{
  public void Configure(EntityTypeBuilder<GuildUser> builder)
  {
    builder.ToTable("GuildUsers");

    builder.HasKey(gu => gu.Id);
    builder.Property(gu => gu.Id)
        .HasDefaultValueSql("gen_random_uuid()");

    builder.Property(gu => gu.GuildId).HasColumnType("numeric(20,0)");
    builder.Property(gu => gu.UserId).HasColumnType("numeric(20,0)");

    builder.Property(gu => gu.Username)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(gu => gu.Discriminator)
        .IsRequired()
        .HasMaxLength(10);

    builder.HasOne(gu => gu.Guild)
        .WithMany(g => g.GuildUsers)
        .HasForeignKey(gu => gu.GuildId)
        .HasPrincipalKey(g => g.GuildId) // Important for ulong-typed FK
        .OnDelete(DeleteBehavior.Cascade);
  }
}
