using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Infrastructure.Teams;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams", table => table.HasCheckConstraint("ck_teams_display_order_non_negative", "display_order >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(SettingsNameRules.NameMaxLength).IsRequired();
        builder.Property(x => x.NormalizedName).HasColumnName("normalized_name").HasMaxLength(SettingsNameRules.NameMaxLength).IsRequired();
        builder.HasIndex(x => x.NormalizedName).IsUnique();
        builder.Property(x => x.TrackingLevel).HasColumnName("tracking_level").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.DisplayOrder).HasColumnName("display_order").IsRequired();
        builder.HasIndex(x => new { x.Status, x.DisplayOrder });
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
    }
}
