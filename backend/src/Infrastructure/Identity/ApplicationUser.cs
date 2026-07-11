using Microsoft.AspNetCore.Identity;
using PlayerPerformance.Domain.Users;

namespace PlayerPerformance.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.INVITED;

    public bool RequiresPasswordChange { get; set; } = true;

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedUtc {
        get; set;
    }
}
