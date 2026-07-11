using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Staff;
using PlayerPerformance.Domain.Settings;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<StaffAccessProfile> StaffAccessProfiles => Set<StaffAccessProfile>();
    public DbSet<StaffTeamScope> StaffTeamScopes => Set<StaffTeamScope>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Opponent> Opponents => Set<Opponent>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerTeamAssignment> PlayerTeamAssignments => Set<PlayerTeamAssignment>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchLineup> MatchLineups => Set<MatchLineup>();
    public DbSet<MatchLineupEntry> MatchLineupEntries => Set<MatchLineupEntry>();
    public DbSet<PlayerMatchAppearance> PlayerMatchAppearances => Set<PlayerMatchAppearance>();
    public DbSet<MatchSubstitution> MatchSubstitutions => Set<MatchSubstitution>();
    public DbSet<MatchReport> MatchReports => Set<MatchReport>();
    public DbSet<PlayerMatchStats> PlayerMatchStats => Set<PlayerMatchStats>();
    public DbSet<GoalkeeperMatchStats> GoalkeeperMatchStats => Set<GoalkeeperMatchStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable(IdentityTableNames.UserClaims);
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable(IdentityTableNames.UserLogins);
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable(IdentityTableNames.UserTokens);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
