using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchLineupConfiguration : IEntityTypeConfiguration<MatchLineup>
{
    public void Configure(EntityTypeBuilder<MatchLineup> builder)
    {
        builder.ToTable("match_lineups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MatchId).HasColumnName("match_id").IsRequired();
        builder.Property(x => x.Formation).HasColumnName("formation").HasMaxLength(32);
        builder.Property(x => x.CaptainPlayerId).HasColumnName("captain_player_id");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(x => x.MatchId).IsUnique();
        builder.HasOne<Match>().WithOne().HasForeignKey<MatchLineup>(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.CaptainPlayerId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MatchLineupEntryConfiguration : IEntityTypeConfiguration<MatchLineupEntry>
{
    public void Configure(EntityTypeBuilder<MatchLineupEntry> builder)
    {
        builder.ToTable("match_lineup_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MatchId).HasColumnName("match_id").IsRequired();
        builder.Property(x => x.PlayerId).HasColumnName("player_id").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(x => new { x.MatchId, x.PlayerId }).IsUnique();
        builder.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PlayerMatchAppearanceConfiguration : IEntityTypeConfiguration<PlayerMatchAppearance>
{
    public void Configure(EntityTypeBuilder<PlayerMatchAppearance> builder)
    {
        builder.ToTable("player_match_appearances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MatchId).HasColumnName("match_id").IsRequired();
        builder.Property(x => x.PlayerId).HasColumnName("player_id").IsRequired();
        builder.Property(x => x.MinutesPlayed).HasColumnName("minutes_played").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(x => new { x.MatchId, x.PlayerId }).IsUnique();
        builder.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MatchSubstitutionConfiguration : IEntityTypeConfiguration<MatchSubstitution>
{
    public void Configure(EntityTypeBuilder<MatchSubstitution> builder)
    {
        builder.ToTable("match_substitutions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MatchId).HasColumnName("match_id").IsRequired();
        builder.Property(x => x.PlayerOutId).HasColumnName("player_out_id").IsRequired();
        builder.Property(x => x.PlayerInId).HasColumnName("player_in_id").IsRequired();
        builder.Property(x => x.Minute).HasColumnName("minute").IsRequired();
        builder.Property(x => x.StoppageTimeMinute).HasColumnName("stoppage_time_minute");
        builder.Property(x => x.Sequence).HasColumnName("sequence").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(x => new { x.MatchId, x.Sequence }).IsUnique();
        builder.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerOutId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerInId).OnDelete(DeleteBehavior.Restrict);
    }
}
