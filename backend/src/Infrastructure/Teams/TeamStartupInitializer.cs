using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Teams;

public sealed class TeamStartupInitializer(AppDbContext dbContext, ISystemClock clock)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (await dbContext.Teams.AnyAsync(ct)) return;
        await using var transaction = dbContext.Database.IsRelational() ? await dbContext.Database.BeginTransactionAsync(ct) : null;
        if (await dbContext.Teams.AnyAsync(ct)) return;
        var now = clock.UtcNow;
        var defaults = new (string Name, TeamTrackingLevel Level)[] { ("First Team", TeamTrackingLevel.FULL), ("U19", TeamTrackingLevel.STANDARD), ("U17", TeamTrackingLevel.STANDARD), ("U15", TeamTrackingLevel.BASIC), ("U13", TeamTrackingLevel.BASIC), ("U11", TeamTrackingLevel.BASIC) };
        for (var order = 0; order < defaults.Length; order++)
        {
            var item = defaults[order];
            dbContext.Teams.Add(Team.Create(Guid.NewGuid(), item.Name, item.Name.ToUpperInvariant(), item.Level, order, now));
        }
        await dbContext.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
    }
}
