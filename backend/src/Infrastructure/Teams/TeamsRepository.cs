using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlayerPerformance.Application.Teams;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Teams;

internal sealed class TeamsRepository(AppDbContext dbContext) : ITeamsRepository
{
    public async Task<IReadOnlyList<Team>> ListAsync(bool includeArchived, CancellationToken ct) => await dbContext.Teams.AsNoTracking().Where(x => includeArchived || x.Status != TeamStatus.ARCHIVED).OrderBy(x => x.DisplayOrder).ThenBy(x => x.NormalizedName).ThenBy(x => x.Id).ToListAsync(ct);
    public async Task<IReadOnlyList<Team>> ListNonArchivedTrackedAsync(CancellationToken ct) => await dbContext.Teams.Where(x => x.Status != TeamStatus.ARCHIVED).OrderBy(x => x.DisplayOrder).ThenBy(x => x.NormalizedName).ThenBy(x => x.Id).ToListAsync(ct);
    public Task<Team?> GetAsync(Guid id, CancellationToken ct) => dbContext.Teams.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> NameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct) => dbContext.Teams.AnyAsync(x => x.NormalizedName == normalizedName && (!excludingId.HasValue || x.Id != excludingId.Value), ct);
    public void Add(Team team) => dbContext.Teams.Add(team);
    public async Task SaveChangesAsync(CancellationToken ct) { try { await dbContext.SaveChangesAsync(ct); } catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { throw new TeamDuplicateNameException(); } }
}
