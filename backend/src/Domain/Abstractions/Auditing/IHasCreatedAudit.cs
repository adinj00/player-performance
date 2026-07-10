namespace PlayerPerformance.Domain.Abstractions.Auditing;

public interface IHasCreatedAudit
{
    DateTime CreatedAtUtc
    {
        get;
    }

    string? CreatedBy
    {
        get;
    }
}
