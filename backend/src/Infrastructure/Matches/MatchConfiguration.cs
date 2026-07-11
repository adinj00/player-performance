using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SeasonId).HasColumnName("season_id").IsRequired();
        builder.Property(x => x.CompetitionId).HasColumnName("competition_id").IsRequired();
        builder.Property(x => x.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(x => x.OpponentId).HasColumnName("opponent_id").IsRequired();
        builder.Property(x => x.VenueId).HasColumnName("venue_id");
        builder.Property(x => x.KickoffAtUtc).HasColumnName("kickoff_at_utc").IsRequired();
        builder.Property(x => x.Round).HasColumnName("round").HasMaxLength(80);
        builder.Property(x => x.LocationType).HasColumnName("location_type").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.TeamScore).HasColumnName("team_score");
        builder.Property(x => x.OpponentScore).HasColumnName("opponent_score");
        builder.Property(x => x.IsArchived).HasColumnName("is_archived").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasOne<PlayerPerformance.Domain.Settings.Season>().WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlayerPerformance.Domain.Settings.Competition>().WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlayerPerformance.Domain.Teams.Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlayerPerformance.Domain.Settings.Opponent>().WithMany().HasForeignKey(x => x.OpponentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlayerPerformance.Domain.Settings.Venue>().WithMany().HasForeignKey(x => x.VenueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TeamId, x.KickoffAtUtc });
        builder.HasIndex(x => new { x.SeasonId, x.KickoffAtUtc });
        builder.HasIndex(x => x.CompetitionId);
        builder.HasIndex(x => x.OpponentId);
        builder.HasIndex(x => new { x.Status, x.IsArchived });
    }
}
