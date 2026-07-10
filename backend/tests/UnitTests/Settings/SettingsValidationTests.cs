using FluentValidation;
using PlayerPerformance.Application.Settings;

namespace PlayerPerformance.UnitTests.Settings;

public sealed class SettingsValidationTests
{
    [Fact]
    public async Task SeasonValidator_ShouldRejectBlankAndOversizedNames()
    {
        var validator = new CreateSeasonRequestValidator();
        Assert.False((await validator.ValidateAsync(new CreateSeasonRequest(" ", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)))).IsValid);
        Assert.False((await validator.ValidateAsync(new CreateSeasonRequest(new string('x', SettingsNameRules.NameMaxLength + 1), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)))).IsValid);
    }

    [Fact]
    public void Normalization_ShouldTrimAndIgnoreCase() => Assert.Equal("PREMIER LIGA", SettingsNameRules.Normalize("  Premier Liga  "));
}
