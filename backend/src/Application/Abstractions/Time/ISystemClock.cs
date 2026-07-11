namespace PlayerPerformance.Application.Abstractions.Time;

public interface ISystemClock
{
    DateTime UtcNow {
        get;
    }
}
