using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.Infrastructure.Files;

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("stored_files", table => table.HasCheckConstraint("CK_stored_files_size_bytes_positive", "size_bytes > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
        builder.Property(x => x.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(StoredFile.OriginalFileNameMaxLength).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(StoredFile.ContentTypeMaxLength).IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.IsArchived).HasColumnName("is_archived").IsRequired();
        builder.Property(x => x.ArchivedAtUtc).HasColumnName("archived_at_utc");
        builder.Property(x => x.ArchivedByUserId).HasColumnName("archived_by_user_id");
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.HasIndex(x => new { x.UploadedByUserId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.IsArchived, x.ArchivedAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ArchivedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_stored_files_archive_metadata_consistent",
            "(is_archived = FALSE AND archived_at_utc IS NULL AND archived_by_user_id IS NULL) OR (is_archived = TRUE AND archived_at_utc IS NOT NULL AND archived_by_user_id IS NOT NULL)"));
    }
}
