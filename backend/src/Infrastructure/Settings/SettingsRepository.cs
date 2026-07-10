using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Domain.Settings;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Settings;

internal sealed class SettingsRepository(AppDbContext dbContext) : ISettingsRepository
{
    public async Task<IReadOnlyList<Season>> ListSeasonsAsync(bool includeArchived, CancellationToken ct) => await dbContext.Seasons.AsNoTracking().Where(x => includeArchived || !x.IsArchived).OrderByDescending(x => x.StartDate).ThenBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct);
    public Task<Season?> GetSeasonAsync(Guid id, CancellationToken ct) => dbContext.Seasons.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> SeasonNameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct) => dbContext.Seasons.AnyAsync(x => x.NormalizedName == normalizedName && (!excludingId.HasValue || x.Id != excludingId.Value), ct);
    public void Add(Season season) => dbContext.Seasons.Add(season);
    public async Task<IReadOnlyList<Competition>> ListCompetitionsAsync(bool includeArchived, CancellationToken ct) => await dbContext.Competitions.AsNoTracking().Where(x => includeArchived || !x.IsArchived).OrderBy(x => x.NormalizedName).ThenBy(x => x.Id).ToListAsync(ct);
    public Task<Competition?> GetCompetitionAsync(Guid id, CancellationToken ct) => dbContext.Competitions.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> CompetitionNameExistsAsync(string normalizedName, Guid? excludingId, CancellationToken ct) => dbContext.Competitions.AnyAsync(x => x.NormalizedName == normalizedName && (!excludingId.HasValue || x.Id != excludingId.Value), ct);
    public void Add(Competition competition) => dbContext.Competitions.Add(competition);
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { throw new SettingsDuplicateNameException(); }
    }
}
