using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;
namespace PlayerPerformance.Domain.Physical;

public enum PhysicalWorkloadSourceKind
{
    IMPORT,
    MANUAL
}
public sealed class PlayerPhysicalWorkload : Entity
{
    private PlayerPhysicalWorkload() : base(Guid.Empty)
    {
    }
    private PlayerPhysicalWorkload(Guid id, Guid team, Guid player, DateOnly date, Guid? participant, Guid? appearance, DateTime now) : base(id)
    {
        TeamId = team;
        PlayerId = player;
        OccurredOn = date;
        TrainingSessionParticipantId = participant;
        PlayerMatchAppearanceId = appearance;
        CreatedAtUtc = now;
    }
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }
    public DateOnly OccurredOn { get; private set; }
    public Guid? TrainingSessionParticipantId { get; private set; }
    public Guid? PlayerMatchAppearanceId { get; private set; }
    public Guid? CurrentRevisionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static PlayerPhysicalWorkload Create(Guid id, Guid team, Guid player, DateOnly date, Guid? participant, Guid? appearance, DateTime now) { Guard.AgainstDefault(id, nameof(id)); Guard.AgainstDefault(team, nameof(team)); Guard.AgainstDefault(player, nameof(player)); if (participant.HasValue == appearance.HasValue) throw new ArgumentException("Exactly one workload context is required."); return new(id, team, player, date, participant, appearance, now); }
    public void SetCurrentRevision(Guid revisionId)
    {
        Guard.AgainstDefault(revisionId, nameof(revisionId));
        CurrentRevisionId = revisionId;
    }
}
public sealed class PhysicalWorkloadRevision : Entity
{
    private PhysicalWorkloadRevision() : base(Guid.Empty) { }
    private PhysicalWorkloadRevision(Guid id, Guid workload, int number, Guid importJob, string source, string key, string version, Guid actor, DateTime now) : base(id)
    {
        PlayerPhysicalWorkloadId = workload;
        RevisionNumber = number;
        SourceKind = PhysicalWorkloadSourceKind.IMPORT;
        ImportJobId = importJob;
        SourceSystem = source;
        ProcessorKey = key;
        ProcessorVersion = version;
        RecordedByUserId = actor;
        RecordedAtUtc = CreatedAtUtc = now;
    }
    public Guid PlayerPhysicalWorkloadId { get; private set; }
    public int RevisionNumber { get; private set; }
    public PhysicalWorkloadSourceKind SourceKind { get; private set; }
    public Guid? ImportJobId { get; private set; }
    public string? SourceSystem { get; private set; }
    public string? ProcessorKey { get; private set; }
    public string? ProcessorVersion { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public static PhysicalWorkloadRevision CreateImport(Guid id, Guid workload, int number, Guid importJob, string source, string key, string version, Guid actor, DateTime now)
    {
        if (number < 1 || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(version))
            throw new ArgumentOutOfRangeException();
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(workload, nameof(workload));
        Guard.AgainstDefault(importJob, nameof(importJob));
        Guard.AgainstDefault(actor, nameof(actor));
        return new(id, workload, number, importJob, source.Trim(), key.Trim(), version.Trim(), actor, now);
    }
}
public sealed class PhysicalMetricValue : Entity
{
    private PhysicalMetricValue() : base(Guid.Empty)
    {
    }
    private PhysicalMetricValue(Guid id, Guid revision, string code, decimal value, PhysicalMetricUnit unit, decimal? threshold, PhysicalMetricUnit? thresholdUnit, ThresholdDirection? direction, ThresholdScope? scope, string? methodKey, string? methodVersion) : base(id)
    {
        PhysicalWorkloadRevisionId = revision;
        MetricCode = code;
        Value = value;
        UnitCode = unit;
        ThresholdValue = threshold;
        ThresholdUnitCode = thresholdUnit;
        ThresholdDirection = direction;
        ThresholdScope = scope;
        MethodKey = methodKey;
        MethodVersion = methodVersion;
    }
    public Guid PhysicalWorkloadRevisionId { get; private set; }
    public string MetricCode { get; private set; } = null!; public decimal Value { get; private set; }
    public PhysicalMetricUnit UnitCode { get; private set; }
    public decimal? ThresholdValue { get; private set; }
    public PhysicalMetricUnit? ThresholdUnitCode { get; private set; }
    public ThresholdDirection? ThresholdDirection { get; private set; }
    public ThresholdScope? ThresholdScope { get; private set; }
    public string? MethodKey { get; private set; }
    public string? MethodVersion { get; private set; }
    public static PhysicalMetricValue Create(Guid id, Guid revision, string code, decimal value, PhysicalMetricUnit unit, decimal? threshold, PhysicalMetricUnit? thresholdUnit, ThresholdDirection? direction, ThresholdScope? scope, string? methodKey, string? methodVersion)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(revision, nameof(revision));
        if (string.IsNullOrWhiteSpace(code) || value < 0 || threshold is < 0 || (threshold.HasValue != thresholdUnit.HasValue || threshold.HasValue != direction.HasValue || threshold.HasValue != scope.HasValue) || (string.IsNullOrWhiteSpace(methodKey) != string.IsNullOrWhiteSpace(methodVersion)))
            throw new ArgumentOutOfRangeException();
        return new(id, revision, code.Trim(), value, unit, threshold, thresholdUnit, direction, scope, methodKey?.Trim(), methodVersion?.Trim());
    }
}
