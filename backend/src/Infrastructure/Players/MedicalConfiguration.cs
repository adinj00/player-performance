using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Medical;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayerAvailabilityConfiguration : IEntityTypeConfiguration<PlayerAvailability>
{
    public void Configure(EntityTypeBuilder<PlayerAvailability> b)
    {
        b.ToTable("player_availabilities");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlayerId).HasColumnName("player_id");
        b.Property(x => x.TeamId).HasColumnName("team_id");
        b.Property(x => x.CurrentRevisionId).HasColumnName("current_revision_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new { x.PlayerId, x.TeamId }).IsUnique();
        b.HasIndex(x => x.TeamId);
        b.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class PlayerAvailabilityRevisionConfiguration : IEntityTypeConfiguration<PlayerAvailabilityRevision>
{
    public void Configure(EntityTypeBuilder<PlayerAvailabilityRevision> b)
    {
        b.ToTable("player_availability_revisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlayerAvailabilityId).HasColumnName("player_availability_id");
        b.Property(x => x.RevisionNumber).HasColumnName("revision_number");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.EffectiveOn).HasColumnName("effective_on").HasColumnType("date");
        b.Property(x => x.ExpectedReturnOn).HasColumnName("expected_return_on").HasColumnType("date");
        b.Property(x => x.CoachVisibleNote).HasColumnName("coach_visible_note").HasMaxLength(300);
        b.Property(x => x.RecordedByUserId).HasColumnName("recorded_by_user_id");
        b.Property(x => x.RecordedAtUtc).HasColumnName("recorded_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new { x.PlayerAvailabilityId, x.RevisionNumber }).IsUnique();
        b.HasIndex(x => x.RecordedAtUtc);
        b.HasOne<PlayerAvailability>().WithMany().HasForeignKey(x => x.PlayerAvailabilityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class InjuryRecordConfiguration : IEntityTypeConfiguration<InjuryRecord>
{
    public void Configure(EntityTypeBuilder<InjuryRecord> b)
    {
        b.ToTable("injury_records");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlayerId).HasColumnName("player_id");
        b.Property(x => x.TeamId).HasColumnName("team_id");
        b.Property(x => x.OccurredOn).HasColumnName("occurred_on").HasColumnType("date");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.CurrentRevisionId).HasColumnName("current_revision_id");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.ResolvedOn).HasColumnName("resolved_on").HasColumnType("date");
        b.Property(x => x.ResolvedByUserId).HasColumnName("resolved_by_user_id");
        b.Property(x => x.ResolvedAtUtc).HasColumnName("resolved_at_utc");
        b.HasIndex(x => new { x.TeamId, x.Status, x.OccurredOn });
        b.HasIndex(x => new { x.PlayerId, x.OccurredOn });
        b.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class InjuryRecordRevisionConfiguration : IEntityTypeConfiguration<InjuryRecordRevision>
{
    public void Configure(EntityTypeBuilder<InjuryRecordRevision> b)
    {
        b.ToTable("injury_record_revisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.InjuryRecordId).HasColumnName("injury_record_id");
        b.Property(x => x.RevisionNumber).HasColumnName("revision_number");
        b.Property(x => x.BodyArea).HasColumnName("body_area").HasMaxLength(200);
        b.Property(x => x.Diagnosis).HasColumnName("diagnosis").HasMaxLength(500);
        b.Property(x => x.RestrictedNotes).HasColumnName("restricted_notes").HasMaxLength(4000);
        b.Property(x => x.RecordedByUserId).HasColumnName("recorded_by_user_id");
        b.Property(x => x.RecordedAtUtc).HasColumnName("recorded_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new { x.InjuryRecordId, x.RevisionNumber }).IsUnique();
        b.HasIndex(x => x.RecordedAtUtc);
        b.HasOne<InjuryRecord>().WithMany().HasForeignKey(x => x.InjuryRecordId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
