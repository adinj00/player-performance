using PlayerPerformance.Domain.Common.Entities;

namespace PlayerPerformance.Domain.Settings;

public sealed class Competition : Entity
{
    private Competition() : base(Guid.Empty) { Name = string.Empty; NormalizedName = string.Empty; }
    private Competition(Guid id, string name, string normalizedName, DateTime utcNow) : base(id) { Name = name; NormalizedName = normalizedName; CreatedAtUtc = utcNow; UpdatedAtUtc = utcNow; }
    public string Name
    {
        get; private set;
    }
    public string NormalizedName
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
    public static Competition Create(Guid id, string name, string normalizedName, DateTime utcNow) => new(id, name, normalizedName, utcNow);
    public void Update(string name, string normalizedName, DateTime utcNow)
    {
        EnsureNotArchived();
        Name = name;
        NormalizedName = normalizedName;
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
    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new InvalidOperationException("Archived competitions cannot be updated.");
    }
}
