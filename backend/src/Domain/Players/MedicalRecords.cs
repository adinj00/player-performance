using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Medical;

public enum AvailabilityStatus
{
    AVAILABLE,
    LIMITED,
    UNAVAILABLE,
    REHAB,
    UNKNOWN
}
public enum InjuryStatus
{
    OPEN,
    RESOLVED
}

public sealed class PlayerAvailability : Entity
{
    private PlayerAvailability() : base(Guid.Empty)
    {
    }
    private PlayerAvailability(Guid id, Guid playerId, Guid teamId, DateTime utcNow) : base(id)
    {
        PlayerId = playerId;
        TeamId = teamId;
        CreatedAtUtc = utcNow;
    }
    public Guid PlayerId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid CurrentRevisionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static PlayerAvailability Create(Guid id, Guid playerId, Guid teamId, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(playerId, nameof(playerId));
        Guard.AgainstDefault(teamId, nameof(teamId));
        return new(id, playerId, teamId, utcNow);
    }
    public void SetCurrentRevision(Guid id)
    {
        Guard.AgainstDefault(id, nameof(id));
        CurrentRevisionId = id;
    }
}
public sealed class PlayerAvailabilityRevision : Entity
{
    private PlayerAvailabilityRevision() : base(Guid.Empty)
    {
    }
    private PlayerAvailabilityRevision(Guid id, Guid parent, int number, AvailabilityStatus status, DateOnly effectiveOn, DateOnly? expectedReturnOn, string? note, Guid actor, DateTime utcNow) : base(id)
    {
        PlayerAvailabilityId = parent;
        RevisionNumber = number;
        Status = status;
        EffectiveOn = effectiveOn;
        ExpectedReturnOn = expectedReturnOn;
        CoachVisibleNote = note;
        RecordedByUserId = actor;
        RecordedAtUtc = utcNow;
        CreatedAtUtc = utcNow;
    }
    public Guid PlayerAvailabilityId { get; private set; }
    public int RevisionNumber { get; private set; }
    public AvailabilityStatus Status { get; private set; }
    public DateOnly EffectiveOn { get; private set; }
    public DateOnly? ExpectedReturnOn { get; private set; }
    public string? CoachVisibleNote { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static PlayerAvailabilityRevision Create(Guid id, Guid parent, int number, AvailabilityStatus status, DateOnly effectiveOn, DateOnly? expectedReturnOn, string? note, Guid actor, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(parent, nameof(parent));
        Guard.AgainstDefault(actor, nameof(actor));
        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (number <= 0 || !Enum.IsDefined(status) || note?.Length > 300 || (status is AvailabilityStatus.AVAILABLE or AvailabilityStatus.UNKNOWN && expectedReturnOn is not null) || (expectedReturnOn is not null && expectedReturnOn < effectiveOn))
            throw new ArgumentOutOfRangeException(nameof(number));
        return new(id, parent, number, status, effectiveOn, expectedReturnOn, note, actor, utcNow);
    }
}
public sealed class InjuryRecord : Entity
{
    private InjuryRecord() : base(Guid.Empty)
    {
    }
    private InjuryRecord(Guid id, Guid playerId, Guid teamId, DateOnly occurredOn, Guid creator, DateTime utcNow) : base(id)
    {
        PlayerId = playerId;
        TeamId = teamId;
        OccurredOn = occurredOn;
        Status = InjuryStatus.OPEN;
        CreatedByUserId = creator;
        CreatedAtUtc = utcNow;
    }
    public Guid PlayerId { get; private set; }
    public Guid TeamId { get; private set; }
    public DateOnly OccurredOn { get; private set; }
    public InjuryStatus Status { get; private set; }
    public Guid CurrentRevisionId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateOnly? ResolvedOn { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public static InjuryRecord Create(Guid id, Guid playerId, Guid teamId, DateOnly occurredOn, Guid creator, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(playerId, nameof(playerId));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(creator, nameof(creator));
        return new(id, playerId, teamId, occurredOn, creator, utcNow);
    }
    public void SetCurrentRevision(Guid id)
    {
        Guard.AgainstDefault(id, nameof(id));
        CurrentRevisionId = id;
    }
    public void Resolve(DateOnly date, Guid actor, DateTime utcNow)
    {
        Guard.AgainstDefault(actor, nameof(actor));
        if (Status != InjuryStatus.OPEN || date < OccurredOn)
            throw new InvalidOperationException();
        Status = InjuryStatus.RESOLVED;
        ResolvedOn = date;
        ResolvedByUserId = actor;
        ResolvedAtUtc = utcNow;
    }
}
public sealed class InjuryRecordRevision : Entity
{
    private InjuryRecordRevision() : base(Guid.Empty)
    {
    }
    private InjuryRecordRevision(Guid id, Guid parent, int number, string? bodyArea, string? diagnosis, string? notes, Guid actor, DateTime utcNow) : base(id)
    {
        InjuryRecordId = parent;
        RevisionNumber = number;
        BodyArea = bodyArea;
        Diagnosis = diagnosis;
        RestrictedNotes = notes;
        RecordedByUserId = actor;
        RecordedAtUtc = utcNow;
        CreatedAtUtc = utcNow;
    }
    public Guid InjuryRecordId { get; private set; }
    public int RevisionNumber { get; private set; }
    public string? BodyArea { get; private set; }
    public string? Diagnosis { get; private set; }
    public string? RestrictedNotes { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static InjuryRecordRevision Create(Guid id, Guid parent, int number, string? bodyArea, string? diagnosis, string? notes, Guid actor, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(parent, nameof(parent));
        Guard.AgainstDefault(actor, nameof(actor));
        bodyArea = Normal(bodyArea, 200);
        diagnosis = Normal(diagnosis, 500);
        notes = Normal(notes, 4000);
        if (number <= 0 || (bodyArea is null && diagnosis is null && notes is null))
            throw new ArgumentOutOfRangeException(nameof(number));
        return new(id, parent, number, bodyArea, diagnosis, notes, actor, utcNow);
    }
    private static string? Normal(string? value, int max)
    {
        value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (value?.Length > max)
            throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
}
