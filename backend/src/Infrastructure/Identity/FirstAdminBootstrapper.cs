using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Users;

namespace PlayerPerformance.Infrastructure.Identity;

public sealed class FirstAdminBootstrapper(
    UserManager<ApplicationUser> userManager,
    IOptions<FirstAdminBootstrapOptions> options,
    ISystemClock clock)
{
    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        if (await userManager.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var bootstrapOptions = options.Value;
        var validationError = bootstrapOptions.GetEmptyStoreValidationError();

        if (validationError is not null)
        {
            throw new InvalidOperationException(validationError);
        }

        var email = bootstrapOptions.Email!;
        var now = clock.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            AccountStatus = UserAccountStatus.ACTIVE,
            RequiresPasswordChange = true,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        var result = await userManager.CreateAsync(user, bootstrapOptions.TemporaryPassword!);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"First admin bootstrap could not create the account. {errors}");
        }
    }
}
