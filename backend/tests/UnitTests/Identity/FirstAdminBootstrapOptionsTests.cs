using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.UnitTests.Identity;

public sealed class FirstAdminBootstrapOptionsTests
{
    [Fact]
    public void GetEmptyStoreValidationError_ShouldRequireEnabledBootstrap()
    {
        var options = new FirstAdminBootstrapOptions();

        var error = options.GetEmptyStoreValidationError();

        Assert.NotNull(error);
        Assert.DoesNotContain("TemporaryPassword", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void GetEmptyStoreValidationError_ShouldRejectMissingOrInvalidEmail(string? email)
    {
        var options = new FirstAdminBootstrapOptions
        {
            Enabled = true,
            Name = "Administrator",
            Email = email,
            TemporaryPassword = "Valid-temporary-password"
        };

        var error = options.GetEmptyStoreValidationError();

        Assert.NotNull(error);
        Assert.Contains("Email", error, StringComparison.Ordinal);
    }

    [Fact]
    public void GetEmptyStoreValidationError_ShouldRequireTemporaryPasswordWithoutExposingItsValue()
    {
        var options = new FirstAdminBootstrapOptions
        {
            Enabled = true,
            Name = "Administrator",
            Email = "admin@example.com"
        };

        var error = options.GetEmptyStoreValidationError();

        Assert.NotNull(error);
        Assert.Contains("TemporaryPassword", error, StringComparison.Ordinal);
    }

    [Fact]
    public void GetEmptyStoreValidationError_ShouldAcceptCompleteConfiguration()
    {
        var options = new FirstAdminBootstrapOptions
        {
            Enabled = true,
            Name = "Administrator",
            Email = "admin@example.com",
            TemporaryPassword = "Valid-temporary-password"
        };

        Assert.Null(options.GetEmptyStoreValidationError());
    }
}
