using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Players;

public sealed class Player : Entity
{
    private Player() : base(Guid.Empty)
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private Player(Guid id, string firstName, string lastName, string? preferredName, DateOnly? dateOfBirth, DateTime utcNow) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        PreferredName = preferredName;
        DateOfBirth = dateOfBirth;
        Status = PlayerRecordStatus.ACTIVE;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PreferredName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public PlayerRecordStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Player Create(Guid id, string firstName, string lastName, string? preferredName, DateOnly? dateOfBirth, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        EnsureName(firstName, nameof(firstName));
        EnsureName(lastName, nameof(lastName));
        EnsureDateOfBirth(dateOfBirth, utcNow);
        return new Player(id, firstName.Trim(), lastName.Trim(), NormalizePreferredName(preferredName), dateOfBirth, utcNow);
    }

    public void UpdateProfile(string firstName, string lastName, string? preferredName, DateOnly? dateOfBirth, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureName(firstName, nameof(firstName));
        EnsureName(lastName, nameof(lastName));
        EnsureDateOfBirth(dateOfBirth, utcNow);
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PreferredName = NormalizePreferredName(preferredName);
        DateOfBirth = dateOfBirth;
        UpdatedAtUtc = utcNow;
    }

    public void Activate(DateTime utcNow)
    {
        EnsureNotArchived();
        if (Status == PlayerRecordStatus.INACTIVE)
            SetStatus(PlayerRecordStatus.ACTIVE, utcNow);
    }

    public void Deactivate(DateTime utcNow)
    {
        EnsureNotArchived();
        if (Status == PlayerRecordStatus.ACTIVE)
            SetStatus(PlayerRecordStatus.INACTIVE, utcNow);
    }

    public void Archive(DateTime utcNow)
    {
        if (Status is PlayerRecordStatus.ACTIVE or PlayerRecordStatus.INACTIVE)
            SetStatus(PlayerRecordStatus.ARCHIVED, utcNow);
    }

    public void Restore(DateTime utcNow)
    {
        if (Status == PlayerRecordStatus.ARCHIVED)
            SetStatus(PlayerRecordStatus.INACTIVE, utcNow);
    }

    private void SetStatus(PlayerRecordStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureNotArchived()
    {
        if (Status == PlayerRecordStatus.ARCHIVED)
            throw new InvalidOperationException("Archived players must be restored before they can be changed.");
    }

    private static void EnsureName(string? name, string parameterName) => Guard.AgainstNullOrWhiteSpace(name, parameterName);

    private static void EnsureDateOfBirth(DateOnly? dateOfBirth, DateTime utcNow)
    {
        if (dateOfBirth > DateOnly.FromDateTime(utcNow))
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "Date of birth cannot be in the future.");
    }

    private static string? NormalizePreferredName(string? preferredName) => string.IsNullOrWhiteSpace(preferredName) ? null : preferredName.Trim();
}
