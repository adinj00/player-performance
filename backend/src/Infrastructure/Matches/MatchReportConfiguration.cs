using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchReportConfiguration : IEntityTypeConfiguration<MatchReport>
{
    public void Configure(EntityTypeBuilder<MatchReport> builder)
    {
        builder.ToTable("match_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MatchId).HasColumnName("match_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(x => x.SubmittedByUserId).HasColumnName("submitted_by_user_id");
        builder.Property(x => x.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(x => x.VerifiedByUserId).HasColumnName("verified_by_user_id");
        builder.Property(x => x.VerifiedAtUtc).HasColumnName("verified_at_utc");
        builder.Property(x => x.LastCorrectionRequestedByUserId).HasColumnName("last_correction_requested_by_user_id");
        builder.Property(x => x.LastCorrectionRequestedAtUtc).HasColumnName("last_correction_requested_at_utc");
        builder.Property(x => x.LastCorrectionReason).HasColumnName("last_correction_reason").HasMaxLength(MatchReport.MaxCorrectionReasonLength);
        builder.Property(x => x.ArchivedByUserId).HasColumnName("archived_by_user_id");
        builder.Property(x => x.ArchivedAtUtc).HasColumnName("archived_at_utc");
        builder.Property(x => x.AppliedTrackingLevel).HasColumnName("applied_tracking_level").HasConversion<string>().HasMaxLength(16);
        builder.HasIndex(x => x.MatchId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.UpdatedAtUtc });
        builder.HasOne<Match>().WithOne().HasForeignKey<MatchReport>(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
