using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class PlayerMatchStatsConfiguration : IEntityTypeConfiguration<PlayerMatchStats>
{
    public void Configure(EntityTypeBuilder<PlayerMatchStats> b)
    {
        b.ToTable("player_match_stats");
        b.HasKey(x => x.Id);
        b.Property(x => x.MatchReportId).HasColumnName("match_report_id").IsRequired();
        b.Property(x => x.PlayerMatchAppearanceId).HasColumnName("player_match_appearance_id").IsRequired();
        b.Property(x => x.Goals).HasColumnName("goals");
        b.Property(x => x.Assists).HasColumnName("assists");
        b.Property(x => x.YellowCards).HasColumnName("yellow_cards");
        b.Property(x => x.RedCards).HasColumnName("red_cards");
        b.Property(x => x.Shots).HasColumnName("shots");
        b.Property(x => x.ShotsOnTarget).HasColumnName("shots_on_target");
        b.Property(x => x.PassesAttempted).HasColumnName("passes_attempted");
        b.Property(x => x.PassesCompleted).HasColumnName("passes_completed");
        b.Property(x => x.KeyPasses).HasColumnName("key_passes");
        b.Property(x => x.DuelsAttempted).HasColumnName("duels_attempted");
        b.Property(x => x.DuelsWon).HasColumnName("duels_won");
        b.Property(x => x.FoulsCommitted).HasColumnName("fouls_committed");
        b.Property(x => x.FoulsWon).HasColumnName("fouls_won");
        b.Property(x => x.Offsides).HasColumnName("offsides");
        b.Property(x => x.BallRecoveries).HasColumnName("ball_recoveries");
        b.Property(x => x.PossessionLosses).HasColumnName("possession_losses");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.HasIndex(x => x.PlayerMatchAppearanceId).IsUnique();
        b.HasIndex(x => x.MatchReportId);
        b.HasOne<MatchReport>().WithMany().HasForeignKey(x => x.MatchReportId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PlayerMatchAppearance>().WithMany().HasForeignKey(x => x.PlayerMatchAppearanceId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class GoalkeeperMatchStatsConfiguration : IEntityTypeConfiguration<GoalkeeperMatchStats>
{
    public void Configure(EntityTypeBuilder<GoalkeeperMatchStats> b)
    {
        b.ToTable("goalkeeper_match_stats");
        b.HasKey(x => x.Id);
        b.Property(x => x.MatchReportId).HasColumnName("match_report_id").IsRequired();
        b.Property(x => x.PlayerMatchAppearanceId).HasColumnName("player_match_appearance_id").IsRequired();
        b.Property(x => x.Saves).HasColumnName("saves");
        b.Property(x => x.GoalsConceded).HasColumnName("goals_conceded");
        b.Property(x => x.CleanSheet).HasColumnName("clean_sheet");
        b.Property(x => x.PenaltySaves).HasColumnName("penalty_saves");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.HasIndex(x => x.PlayerMatchAppearanceId).IsUnique();
        b.HasIndex(x => x.MatchReportId);
        b.HasOne<MatchReport>().WithMany().HasForeignKey(x => x.MatchReportId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PlayerMatchAppearance>().WithMany().HasForeignKey(x => x.PlayerMatchAppearanceId).OnDelete(DeleteBehavior.Restrict);
    }
}
