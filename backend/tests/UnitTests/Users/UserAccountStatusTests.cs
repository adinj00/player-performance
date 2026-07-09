using PlayerPerformance.Domain.Users;

namespace PlayerPerformance.UnitTests.Users;

public sealed class UserAccountStatusTests
{
    [Fact]
    public void UserAccountStatus_ShouldExposeExpectedLifecycleStatuses()
    {
        var statuses = Enum.GetNames<UserAccountStatus>();

        Assert.Equal(
            ["INVITED", "ACTIVE", "DISABLED", "LOCKED"],
            statuses);
    }
}
