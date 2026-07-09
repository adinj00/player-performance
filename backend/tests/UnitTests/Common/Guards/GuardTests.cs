using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.UnitTests.Common.Guards;

public sealed class GuardTests
{
    [Fact]
    public void AgainstNull_ShouldThrow_WhenValueIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull<string>(null, "value"));
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ShouldThrow_WhenValueIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrWhiteSpace("   ", "value"));
    }

    [Fact]
    public void AgainstDefault_ShouldThrow_WhenGuidIsEmpty()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstDefault(Guid.Empty, "value"));
    }
}
