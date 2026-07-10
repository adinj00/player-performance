using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Infrastructure.Staff;

internal sealed class StaffTeamScopeConfiguration : IEntityTypeConfiguration<StaffTeamScope>
{
    public void Configure(EntityTypeBuilder<StaffTeamScope> builder)
    {
        builder.ToTable("staff_team_scopes");
        builder.HasKey(scope => new { scope.UserId, scope.TeamId });
        builder.Property(scope => scope.CreatedUtc).HasColumnName("created_utc");
        builder.HasIndex(scope => scope.TeamId);
        builder.HasOne<StaffAccessProfile>().WithMany().HasForeignKey(scope => scope.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Team>().WithMany().HasForeignKey(scope => scope.TeamId).OnDelete(DeleteBehavior.Restrict);
    }
}
