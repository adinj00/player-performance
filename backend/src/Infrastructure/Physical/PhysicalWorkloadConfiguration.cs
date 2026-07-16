using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Physical;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Training;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Imports;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Physical;

internal sealed class WorkloadConfiguration : IEntityTypeConfiguration<PlayerPhysicalWorkload>
{
    public void Configure(EntityTypeBuilder<PlayerPhysicalWorkload> b)
    {
        b.ToTable("player_physical_workloads", x => x.HasCheckConstraint("ck_workload_exactly_one_context", "(training_session_participant_id IS NOT NULL) <> (player_match_appearance_id IS NOT NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.TeamId).HasColumnName("team_id");
        b.Property(x => x.PlayerId).HasColumnName("player_id");
        b.Property(x => x.OccurredOn).HasColumnName("occurred_on").HasColumnType("date");
        b.Property(x => x.TrainingSessionParticipantId).HasColumnName("training_session_participant_id");
        b.Property(x => x.PlayerMatchAppearanceId).HasColumnName("player_match_appearance_id");
        b.Property(x => x.CurrentRevisionId).HasColumnName("current_revision_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => x.TrainingSessionParticipantId).IsUnique();
        b.HasIndex(x => x.PlayerMatchAppearanceId).IsUnique();
        b.HasIndex(x => new { x.TeamId, x.OccurredOn });
        b.HasIndex(x => new { x.PlayerId, x.OccurredOn });
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TrainingSessionParticipant>().WithMany().HasForeignKey(x => x.TrainingSessionParticipantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PlayerMatchAppearance>().WithMany().HasForeignKey(x => x.PlayerMatchAppearanceId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class RevisionConfiguration : IEntityTypeConfiguration<PhysicalWorkloadRevision>
{
    public void Configure(EntityTypeBuilder<PhysicalWorkloadRevision> b)
    {
        b.ToTable("physical_workload_revisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlayerPhysicalWorkloadId).HasColumnName("player_physical_workload_id");
        b.Property(x => x.RevisionNumber).HasColumnName("revision_number");
        b.Property(x => x.SourceKind).HasColumnName("source_kind").HasConversion<string>();
        b.Property(x => x.ImportJobId).HasColumnName("import_job_id");
        b.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(100);
        b.Property(x => x.ProcessorKey).HasColumnName("processor_key").HasMaxLength(200);
        b.Property(x => x.ProcessorVersion).HasColumnName("processor_version").HasMaxLength(100);
        b.Property(x => x.RecordedByUserId).HasColumnName("recorded_by_user_id");
        b.Property(x => x.RecordedAtUtc).HasColumnName("recorded_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new { x.PlayerPhysicalWorkloadId, x.RevisionNumber }).IsUnique();
        b.HasIndex(x => x.ImportJobId);
        b.HasOne<PlayerPhysicalWorkload>().WithMany().HasForeignKey(x => x.PlayerPhysicalWorkloadId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ImportJob>().WithMany().HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class MetricConfiguration : IEntityTypeConfiguration<PhysicalMetricValue>
{
    public void Configure(EntityTypeBuilder<PhysicalMetricValue> b)
    {
        b.ToTable("physical_metric_values");
        b.HasKey(x => x.Id);
        b.Property(x => x.PhysicalWorkloadRevisionId).HasColumnName("physical_workload_revision_id");
        b.Property(x => x.MetricCode).HasColumnName("metric_code").HasMaxLength(100);
        b.Property(x => x.Value).HasColumnName("value").HasPrecision(18, 6);
        b.Property(x => x.UnitCode).HasColumnName("unit_code").HasConversion<string>();
        b.Property(x => x.ThresholdValue).HasColumnName("threshold_value").HasPrecision(18, 6);
        b.Property(x => x.ThresholdUnitCode).HasColumnName("threshold_unit_code").HasConversion<string>();
        b.Property(x => x.ThresholdDirection).HasColumnName("threshold_direction").HasConversion<string>();
        b.Property(x => x.ThresholdScope).HasColumnName("threshold_scope").HasConversion<string>();
        b.Property(x => x.MethodKey).HasColumnName("method_key").HasMaxLength(200);
        b.Property(x => x.MethodVersion).HasColumnName("method_version").HasMaxLength(100);
        b.HasIndex(x => new { x.PhysicalWorkloadRevisionId, x.MetricCode }).IsUnique();
        b.HasIndex(x => x.MetricCode);
        b.HasOne<PhysicalWorkloadRevision>().WithMany().HasForeignKey(x => x.PhysicalWorkloadRevisionId).OnDelete(DeleteBehavior.Restrict);
    }
}
