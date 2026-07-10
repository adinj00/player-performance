using PlayerPerformance.Domain.Common.Entities;

namespace PlayerPerformance.Domain.Settings;

public sealed class Season : Entity
{
    private Season() : base(Guid.Empty) { Name = string.Empty; NormalizedName = string.Empty; }
    private Season(Guid id, string name, string normalizedName, DateOnly startDate, DateOnly endDate, DateTime utcNow) : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
    public string Name
    {
        get; private set;
    }
    public string NormalizedName
    {
        get; private set;
    }
    public DateOnly StartDate
    {
        get; private set;
    }
    public DateOnly EndDate
    {
        get; private set;
    }
    public bool IsArchived
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime UpdatedAtUtc
    {
        get; private set;
    }
    public static Season Create(Guid id, string name, string normalizedName, DateOnly startDate, DateOnly endDate, DateTime utcNow)
    {
        EnsureValidDateRange(startDate, endDate);
        return new(id, name, normalizedName, startDate, endDate, utcNow);
    }
    public void Update(string name, string normalizedName, DateOnly startDate, DateOnly endDate, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureValidDateRange(startDate, endDate);
        Name = name;
        NormalizedName = normalizedName;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAtUtc = utcNow;
    }
    public void Archive(DateTime utcNow)
    {
        if (!IsArchived)
        {
            IsArchived = true;
            UpdatedAtUtc = utcNow;
        }
    }
    public void Restore(DateTime utcNow)
    {
        if (IsArchived)
        {
            IsArchived = false;
            UpdatedAtUtc = utcNow;
        }
    }
    private static void EnsureValidDateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Season start date must not be later than its end date.", nameof(startDate));
    }
    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new InvalidOperationException("Archived seasons cannot be updated.");
    }
}
