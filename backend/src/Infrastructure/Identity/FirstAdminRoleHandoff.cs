using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Infrastructure.Persistence;
using PlayerPerformance.Infrastructure.Staff;

namespace PlayerPerformance.Infrastructure.Identity;

public sealed class FirstAdminRoleHandoff(
    AppDbContext dbContext,
    IOptions<FirstAdminBootstrapOptions> options,
    ISystemClock clock)
{
    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var configuredEmail = options.Value.Email?.Trim();
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(configuredEmail))
        {
            var normalizedEmail = configuredEmail.ToUpperInvariant();
            user = await dbContext.Users.SingleOrDefaultAsync(
                candidate => candidate.NormalizedEmail == normalizedEmail,
                cancellationToken);
            if (user is null)
            {
                return;
            }
        }
        else
        {
            var users = await dbContext.Users.Take(2).ToListAsync(cancellationToken);
            var profileCount = await dbContext.StaffAccessProfiles.CountAsync(cancellationToken);
            if (users.Count == 0 || profileCount > 0)
            {
                return;
            }

            if (users.Count != 1)
            {
                throw new InvalidOperationException("First-admin role handoff is ambiguous: configure Bootstrap:FirstAdmin:Email to identify the intended account.");
            }

            user = users[0];
        }

        if (await dbContext.StaffAccessProfiles.AnyAsync(profile => profile.UserId == user.Id, cancellationToken))
        {
            return;
        }

        dbContext.StaffAccessProfiles.Add(new StaffAccessProfile
        {
            UserId = user.Id,
            DisplayName = options.Value.Name!.Trim(),
            PrimaryRole = StaffRole.ADMIN,
            CreatedUtc = clock.UtcNow,
            UpdatedUtc = clock.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
