using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Infrastructure.Identity;
namespace PlayerPerformance.Infrastructure.Auditing;
internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id").IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(64).IsRequired();
        builder.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(32).IsRequired();
        builder.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(x => x.PreviousValuesJson).HasColumnName("previous_values").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.NewValuesJson).HasColumnName("new_values").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb").IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new
        {
            x.EntityType,
            x.EntityId,
            x.OccurredAtUtc
        });
        builder.HasIndex(x => new
        {
            x.ActorUserId,
            x.OccurredAtUtc
        });
        builder.HasIndex(x => new
        {
            x.Action,
            x.OccurredAtUtc
        });
    }
}
