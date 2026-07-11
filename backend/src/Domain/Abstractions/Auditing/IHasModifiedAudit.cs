namespace PlayerPerformance.Domain.Abstractions.Auditing;

public interface IHasModifiedAudit
{
    DateTime? LastModifiedAtUtc {
        get;
    }

    string? LastModifiedBy {
        get;
    }
}
