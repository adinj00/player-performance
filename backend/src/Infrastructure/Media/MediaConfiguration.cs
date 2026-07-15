using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;
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

internal abstract class MediaLinkConfiguration<TLink> : IEntityTypeConfiguration<TLink> where TLink : MediaLink
{
    public virtual void Configure(EntityTypeBuilder<TLink> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.MediaItemId).HasColumnName("media_item_id");
        b.Property(x => x.LinkedByUserId).HasColumnName("linked_by_user_id");
        b.Property(x => x.LinkedAtUtc).HasColumnName("linked_at_utc");
        b.Property(x => x.UnlinkedByUserId).HasColumnName("unlinked_by_user_id");
        b.Property(x => x.UnlinkedAtUtc).HasColumnName("unlinked_at_utc");
        b.HasOne<MediaItem>().WithMany().HasForeignKey(x => x.MediaItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.LinkedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UnlinkedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MediaMatchLinkConfiguration : MediaLinkConfiguration<MediaMatchLink>
{
    public override void Configure(EntityTypeBuilder<MediaMatchLink> b)
    {
        base.Configure(b);
        b.ToTable("media_match_links");
        b.Property(x => x.MatchId).HasColumnName("match_id");
        b.HasIndex(x => new { x.MatchId, x.UnlinkedAtUtc });
        b.HasIndex(x => new { x.MediaItemId, x.MatchId, x.UnlinkedAtUtc }).IsUnique().HasFilter("unlinked_at_utc IS NULL");
        b.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class MediaMatchReportLinkConfiguration : MediaLinkConfiguration<MediaMatchReportLink>
{
    public override void Configure(EntityTypeBuilder<MediaMatchReportLink> b)
    {
        base.Configure(b);
        b.ToTable("media_match_report_links");
        b.Property(x => x.MatchReportId).HasColumnName("match_report_id");
        b.HasIndex(x => new { x.MatchReportId, x.UnlinkedAtUtc });
        b.HasIndex(x => new { x.MediaItemId, x.MatchReportId, x.UnlinkedAtUtc }).IsUnique().HasFilter("unlinked_at_utc IS NULL");
        b.HasOne<MatchReport>().WithMany().HasForeignKey(x => x.MatchReportId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class MediaPlayerLinkConfiguration : MediaLinkConfiguration<MediaPlayerLink>
{
    public override void Configure(EntityTypeBuilder<MediaPlayerLink> b)
    {
        base.Configure(b);
        b.ToTable("media_player_links");
        b.Property(x => x.PlayerId).HasColumnName("player_id");
        b.HasIndex(x => new { x.PlayerId, x.UnlinkedAtUtc });
        b.HasIndex(x => new { x.MediaItemId, x.PlayerId, x.UnlinkedAtUtc }).IsUnique().HasFilter("unlinked_at_utc IS NULL");
        b.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
    }
}
