using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayerTeamAssignmentConfiguration : IEntityTypeConfiguration<PlayerTeamAssignment>
{
    public void Configure(EntityTypeBuilder<PlayerTeamAssignment> builder)
    {
        builder.ToTable("player_team_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlayerId).HasColumnName("player_id").IsRequired();
        builder.Property(x => x.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Team>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PlayerId, x.StartDate, x.EndDate, x.Id });
        builder.HasIndex(x => new { x.TeamId, x.StartDate, x.EndDate, x.PlayerId });
        builder.HasIndex(x => new { x.PlayerId, x.TeamId, x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.PlayerId, x.TeamId }).HasFilter("end_date IS NULL").IsUnique();
    }
}
