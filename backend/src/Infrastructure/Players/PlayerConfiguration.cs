using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(PlayerNameRules.NameMaxLength).IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(PlayerNameRules.NameMaxLength).IsRequired();
        builder.Property(x => x.PreferredName).HasColumnName("preferred_name").HasMaxLength(PlayerNameRules.NameMaxLength);
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(x => new { x.Status, x.LastName, x.FirstName, x.Id });
    }
}
