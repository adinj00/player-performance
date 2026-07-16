using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Domain.Imports;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> b)
    {
        b.ToTable("import_jobs");
        b.HasKey(x => x.Id);
        b.Property(x => x.TeamId).HasColumnName("team_id").IsRequired();
        b.Property(x => x.MatchId).HasColumnName("match_id");
        b.Property(x => x.StoredFileId).HasColumnName("stored_file_id").IsRequired();
        b.Property(x => x.ImportType).HasColumnName("import_type").HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.SourceSystem).HasColumnName("source_system").HasConversion<string>().HasMaxLength(16).IsRequired();
        b.Property(x => x.SourceLabel).HasColumnName("source_label").HasMaxLength(100);
        b.Property(x => x.FileFormat).HasColumnName("file_format").HasConversion<string>().HasMaxLength(8).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        b.Property(x => x.ConfigurationRevision).HasColumnName("configuration_revision").IsConcurrencyToken();
        b.Property(x => x.ValidatedConfigurationRevision).HasColumnName("validated_configuration_revision");
        b.Property(x => x.ValidatedProcessorKey).HasColumnName("validated_processor_key").HasMaxLength(200);
        b.Property(x => x.ValidatedProcessorVersion).HasColumnName("validated_processor_version").HasMaxLength(100);
        b.Property(x => x.ValidatedAtUtc).HasColumnName("validated_at_utc");
        b.Property(x => x.PreviewGeneratedAtUtc).HasColumnName("preview_generated_at_utc");
        b.Property(x => x.ValidationCompletedAtUtc).HasColumnName("validation_completed_at_utc");
        b.Property(x => x.TotalRowCount).HasColumnName("total_row_count");
        b.Property(x => x.PreviewRowCount).HasColumnName("preview_row_count");
        b.Property(x => x.ValidRowCount).HasColumnName("valid_row_count");
        b.Property(x => x.InvalidRowCount).HasColumnName("invalid_row_count");
        b.Property(x => x.WarningCount).HasColumnName("warning_count");
        b.Property(x => x.FailureCode).HasColumnName("failure_code").HasMaxLength(100);
        b.Property(x => x.FailureMessage).HasColumnName("failure_message").HasMaxLength(1000);
        b.Property(x => x.ResultSummaryJson).HasColumnName("result_summary_json").HasColumnType("jsonb");
        b.Property(x => x.PreviewMetadataJson).HasColumnName("preview_metadata_json").HasColumnType("jsonb");
        b.Property(x => x.ProcessingOperation).HasColumnName("processing_operation").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.ProcessingLeaseId).HasColumnName("processing_lease_id");
        b.Property(x => x.ProcessingStartedAtUtc).HasColumnName("processing_started_at_utc");
        b.Property(x => x.ProcessingRequestedByUserId).HasColumnName("processing_requested_by_user_id");
        b.Property(x => x.ProcessingProcessorKey).HasColumnName("processing_processor_key").HasMaxLength(200);
        b.Property(x => x.ProcessingProcessorVersion).HasColumnName("processing_processor_version").HasMaxLength(100);
        b.Property(x => x.ConfirmedByUserId).HasColumnName("confirmed_by_user_id");
        b.Property(x => x.ConfirmedAtUtc).HasColumnName("confirmed_at_utc");
        b.Property(x => x.CancelledByUserId).HasColumnName("cancelled_by_user_id");
        b.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        b.HasIndex(x => x.StoredFileId).IsUnique();
        b.HasIndex(x => new
        {
            x.TeamId,
            x.Status,
            x.CreatedAtUtc
        });
        b.HasIndex(x => new
        {
            x.MatchId,
            x.CreatedAtUtc
        });
        b.HasIndex(x => new
        {
            x.ImportType,
            x.SourceSystem,
            x.CreatedAtUtc
        });
        b.HasIndex(x => new
        {
            x.CreatedByUserId,
            x.CreatedAtUtc
        });
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StoredFile>().WithMany().HasForeignKey(x => x.StoredFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ProcessingRequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ImportPreviewColumnConfiguration : IEntityTypeConfiguration<ImportPreviewColumn>
{
    public void Configure(EntityTypeBuilder<ImportPreviewColumn> b)
    {
        b.ToTable("import_preview_columns");
        b.HasKey(x => x.Id);
        b.Property(x => x.ImportJobId).HasColumnName("import_job_id");
        b.Property(x => x.Ordinal).HasColumnName("ordinal");
        b.Property(x => x.SourceHeader).HasColumnName("source_header").HasMaxLength(500);
        b.Property(x => x.NormalizedHeader).HasColumnName("normalized_header").HasMaxLength(500);
        b.Property(x => x.DetectedDataType).HasColumnName("detected_data_type").HasMaxLength(100);
        b.HasIndex(x => new
        {
            x.ImportJobId,
            x.Ordinal
        }).IsUnique();
        b.HasOne<ImportJob>().WithMany().HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class ImportPreviewRowConfiguration : IEntityTypeConfiguration<ImportPreviewRow>
{
    public void Configure(EntityTypeBuilder<ImportPreviewRow> b)
    {
        b.ToTable("import_preview_rows");
        b.HasKey(x => x.Id);
        b.Property(x => x.ImportJobId).HasColumnName("import_job_id");
        b.Property(x => x.SourceRowNumber).HasColumnName("source_row_number");
        b.Property(x => x.ValuesJson).HasColumnName("values_json").HasColumnType("jsonb");
        b.HasIndex(x => new
        {
            x.ImportJobId,
            x.SourceRowNumber
        });
        b.HasOne<ImportJob>().WithMany().HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class ImportValidationIssueConfiguration : IEntityTypeConfiguration<ImportValidationIssue>
{
    public void Configure(EntityTypeBuilder<ImportValidationIssue> b)
    {
        b.ToTable("import_validation_issues");
        b.HasKey(x => x.Id);
        b.Property(x => x.ImportJobId).HasColumnName("import_job_id");
        b.Property(x => x.Severity).HasColumnName("severity").HasConversion<string>().HasMaxLength(8);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(100);
        b.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000);
        b.Property(x => x.SourceRowNumber).HasColumnName("source_row_number");
        b.Property(x => x.ColumnKey).HasColumnName("column_key").HasMaxLength(500);
        b.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new
        {
            x.ImportJobId,
            x.Severity,
            x.SourceRowNumber
        });
        b.HasOne<ImportJob>().WithMany().HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Cascade);
    }
}
