using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Infrastructure;

namespace PlayerPerformance.UnitTests.Time;

public sealed class SystemClockTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterSystemClock()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    "ConnectionStrings:DefaultConnection",
                    "Host=localhost;Database=test;Username=test;Password=test")
            ])
            .Build();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var clock = provider.GetRequiredService<ISystemClock>();

        Assert.IsType<PlayerPerformance.Infrastructure.Time.SystemClock>(clock);
    }

    [Fact]
    public void UtcNow_ShouldReturnUtcTimestamp()
    {
        var clock = new PlayerPerformance.Infrastructure.Time.SystemClock();

        Assert.Equal(DateTimeKind.Utc, clock.UtcNow.Kind);
    }
}
