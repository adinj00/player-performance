using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.Infrastructure.Settings;

internal sealed class OpponentConfiguration : IEntityTypeConfiguration<Opponent>
{
    public void Configure(EntityTypeBuilder<Opponent> builder)
    {
        builder.ToTable("opponents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(SettingsNameRules.NameMaxLength).IsRequired();
        builder.Property(x => x.NormalizedName).HasColumnName("normalized_name").HasMaxLength(SettingsNameRules.NameMaxLength).IsRequired();
        builder.HasIndex(x => x.NormalizedName).IsUnique();
        builder.Property(x => x.IsArchived).HasColumnName("is_archived").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
    }
}
