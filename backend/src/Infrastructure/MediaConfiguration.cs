using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Media;

internal sealed class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> b)
    {
        b.ToTable("media_items", t => t.HasCheckConstraint("ck_media_items_archive", "(is_archived = FALSE AND archived_at_utc IS NULL AND archived_by_user_id IS NULL) OR (is_archived = TRUE AND archived_at_utc IS NOT NULL AND archived_by_user_id IS NOT NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.TeamId).HasColumnName("team_id");
        b.Property(x => x.SourceType).HasColumnName("source_type").HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Category).HasColumnName("category").HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Title).HasColumnName("title").HasMaxLength(MediaItem.TitleMaxLength);
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(MediaItem.DescriptionMaxLength);
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        b.Property(x => x.IsArchived).HasColumnName("is_archived");
        b.Property(x => x.ArchivedAtUtc).HasColumnName("archived_at_utc");
        b.Property(x => x.ArchivedByUserId).HasColumnName("archived_by_user_id");
        b.HasIndex(x => new
        {
            x.TeamId,
            x.IsArchived,
            x.CreatedAtUtc
        });
        b.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ArchivedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> b)
    {
        b.ToTable("media_assets");
        b.HasKey(x => x.Id);
        b.Property(x => x.MediaItemId).HasColumnName("media_item_id");
        b.Property(x => x.StoredFileId).HasColumnName("stored_file_id");
        b.HasIndex(x => x.MediaItemId).IsUnique();
        b.HasIndex(x => x.StoredFileId).IsUnique();
        b.HasOne<MediaItem>().WithOne().HasForeignKey<MediaAsset>(x => x.MediaItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<StoredFile>().WithOne().HasForeignKey<MediaAsset>(x => x.StoredFileId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ExternalMediaReferenceConfiguration : IEntityTypeConfiguration<ExternalMediaReference>
{
    public void Configure(EntityTypeBuilder<ExternalMediaReference> b)
    {
        b.ToTable("external_media_references");
        b.HasKey(x => x.Id);
        b.Property(x => x.MediaItemId).HasColumnName("media_item_id");
        b.Property(x => x.Url).HasColumnName("url").HasMaxLength(ExternalMediaReference.UrlMaxLength);
        b.Property(x => x.ProviderLabel).HasColumnName("provider_label").HasMaxLength(ExternalMediaReference.ProviderLabelMaxLength);
        b.HasIndex(x => x.MediaItemId).IsUnique();
        b.HasOne<MediaItem>().WithOne().HasForeignKey<ExternalMediaReference>(x => x.MediaItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
