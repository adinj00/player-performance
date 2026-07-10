using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Teams;

public sealed class Team : Entity
{
    private Team() : base(Guid.Empty) { Name = string.Empty; NormalizedName = string.Empty; }
    private Team(Guid id, string name, string normalizedName, TeamTrackingLevel trackingLevel, int displayOrder, DateTime utcNow) : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
        TrackingLevel = trackingLevel;
        Status = TeamStatus.ACTIVE;
        DisplayOrder = displayOrder;
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
    public TeamTrackingLevel TrackingLevel
    {
        get; private set;
    }
    public TeamStatus Status
    {
        get; private set;
    }
    public int DisplayOrder
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
    public static Team Create(Guid id, string name, string normalizedName, TeamTrackingLevel trackingLevel, int displayOrder, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        EnsureName(name, nameof(name));
        EnsureTrackingLevel(trackingLevel);
        EnsureDisplayOrder(displayOrder);
        return new(id, name.Trim(), normalizedName, trackingLevel, displayOrder, utcNow);
    }
    public void Update(string name, string normalizedName, TeamTrackingLevel trackingLevel, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureName(name, nameof(name));
        EnsureTrackingLevel(trackingLevel);
        Name = name.Trim();
        NormalizedName = normalizedName;
        TrackingLevel = trackingLevel;
        UpdatedAtUtc = utcNow;
    }
    public void Activate(DateTime utcNow)
    {
        if (Status == TeamStatus.INACTIVE)
            SetStatus(TeamStatus.ACTIVE, utcNow);
    }
    public void Deactivate(DateTime utcNow)
    {
        if (Status == TeamStatus.ACTIVE)
            SetStatus(TeamStatus.INACTIVE, utcNow);
    }
    public void Archive(DateTime utcNow)
    {
        if (Status is TeamStatus.ACTIVE or TeamStatus.INACTIVE)
            SetStatus(TeamStatus.ARCHIVED, utcNow);
    }
    public void Restore(DateTime utcNow)
    {
        if (Status == TeamStatus.ARCHIVED)
            SetStatus(TeamStatus.INACTIVE, utcNow);
    }
    public void AssignDisplayOrder(int displayOrder, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureDisplayOrder(displayOrder);
        DisplayOrder = displayOrder;
        UpdatedAtUtc = utcNow;
    }
    private void SetStatus(TeamStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAtUtc = utcNow;
    }
    private void EnsureNotArchived()
    {
        if (Status == TeamStatus.ARCHIVED)
            throw new InvalidOperationException("Archived teams must be restored before they can be updated.");
    }
    private static void EnsureName(string? name, string parameterName) => Guard.AgainstNullOrWhiteSpace(name, parameterName);
    private static void EnsureTrackingLevel(TeamTrackingLevel level)
    {
        if (!Enum.IsDefined(level))
            throw new ArgumentOutOfRangeException(nameof(level));
    }
    private static void EnsureDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));
    }
}
