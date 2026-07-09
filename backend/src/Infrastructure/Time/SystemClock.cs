using PlayerPerformance.Application.Abstractions.Time;

namespace PlayerPerformance.Infrastructure.Time;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
