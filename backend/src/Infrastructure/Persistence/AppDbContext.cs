using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Staff;
using PlayerPerformance.Domain.Settings;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<StaffAccessProfile> StaffAccessProfiles => Set<StaffAccessProfile>();
    public DbSet<StaffTeamScope> StaffTeamScopes => Set<StaffTeamScope>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable(IdentityTableNames.UserClaims);
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable(IdentityTableNames.UserLogins);
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable(IdentityTableNames.UserTokens);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
