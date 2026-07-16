using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Training;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Training;

internal sealed class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> b)
    {
        b.ToTable("training_sessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.TeamId).HasColumnName("team_id");
        b.Property(x => x.SessionDate).HasColumnName("session_date").HasColumnType("date");
        b.Property(x => x.StartsAtUtc).HasColumnName("starts_at_utc");
        b.Property(x => x.EndsAtUtc).HasColumnName("ends_at_utc");
        b.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
        b.Property(x => x.Location).HasColumnName("location").HasMaxLength(200);
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        b.Property(x => x.CompletedByUserId).HasColumnName("completed_by_user_id");
        b.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        b.Property(x => x.CancelledByUserId).HasColumnName("cancelled_by_user_id");
        b.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        b.HasIndex(x => new { x.TeamId, x.SessionDate, x.Status });
        b.HasIndex(x => new { x.Status, x.SessionDate });
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class TrainingSessionParticipantConfiguration : IEntityTypeConfiguration<TrainingSessionParticipant>
{
    public void Configure(EntityTypeBuilder<TrainingSessionParticipant> b)
    {
        b.ToTable("training_session_participants");
        b.HasKey(x => x.Id);
        b.Property(x => x.TrainingSessionId).HasColumnName("training_session_id");
        b.Property(x => x.PlayerId).HasColumnName("player_id");
        b.Property(x => x.AddedByUserId).HasColumnName("added_by_user_id");
        b.Property(x => x.AddedAtUtc).HasColumnName("added_at_utc");
        b.Property(x => x.RemovedByUserId).HasColumnName("removed_by_user_id");
        b.Property(x => x.RemovedAtUtc).HasColumnName("removed_at_utc");
        b.HasIndex(x => new { x.TrainingSessionId, x.PlayerId }).HasFilter("removed_at_utc IS NULL").IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.RemovedAtUtc });
        b.HasIndex(x => new { x.PlayerId, x.AddedAtUtc });
        b.HasOne<TrainingSession>().WithMany().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AddedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RemovedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
