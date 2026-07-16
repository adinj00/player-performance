using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;
namespace PlayerPerformance.Domain.Training;

public sealed class TrainingSession : Entity
{
    public const int TitleMaxLength = 200, LocationMaxLength = 200, DescriptionMaxLength = 2000;
    private TrainingSession() : base(Guid.Empty)
    {
    }
    private TrainingSession(Guid id, Guid teamId, DateOnly date, DateTime? starts, DateTime? ends, string title, string? location, string? description, Guid actor, DateTime now) : base(id)
    {
        TeamId = teamId;
        SessionDate = date;
        StartsAtUtc = starts;
        EndsAtUtc = ends;
        Title = title;
        Location = location;
        Description = description;
        CreatedByUserId = actor;
        CreatedAtUtc = UpdatedAtUtc = now;
        Status = TrainingSessionStatus.PLANNED;
    }
    public Guid TeamId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public DateTime? StartsAtUtc { get; private set; }
    public DateTime? EndsAtUtc { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    public TrainingSessionStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public static TrainingSession Create(Guid id, Guid teamId, DateOnly date, DateTime? starts, DateTime? ends, string title, string? location, string? description, Guid actor, DateTime now)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(actor, nameof(actor));
        ValidateTimes(starts, ends);
        return new(id, teamId, date, starts, ends, Required(title, TitleMaxLength), Optional(location, LocationMaxLength), Optional(description, DescriptionMaxLength), actor, now);
    }
    public bool Update(DateOnly date, DateTime? starts, DateTime? ends, string title, string? location, string? description, DateTime now)
    {
        if (Status == TrainingSessionStatus.CANCELLED)
            throw new InvalidOperationException();
        if (Status == TrainingSessionStatus.COMPLETED && (date != SessionDate || starts != StartsAtUtc || ends != EndsAtUtc))
            throw new InvalidOperationException();
        ValidateTimes(starts, ends);
        title = Required(title, TitleMaxLength);
        location = Optional(location, LocationMaxLength);
        description = Optional(description, DescriptionMaxLength);
        if (date == SessionDate && starts == StartsAtUtc && ends == EndsAtUtc && title == Title && location == Location && description == Description)
            return false;
        SessionDate = date;
        StartsAtUtc = starts;
        EndsAtUtc = ends;
        Title = title;
        Location = location;
        Description = description;
        UpdatedAtUtc = now;
        return true;
    }
    public void Complete(Guid actor, DateTime now)
    {
        Guard.AgainstDefault(actor, nameof(actor));
        if (Status != TrainingSessionStatus.PLANNED)
            throw new InvalidOperationException();
        Status = TrainingSessionStatus.COMPLETED;
        CompletedByUserId = actor;
        CompletedAtUtc = UpdatedAtUtc = now;
    }
    public void Cancel(Guid actor, DateTime now)
    {
        Guard.AgainstDefault(actor, nameof(actor));
        if (Status != TrainingSessionStatus.PLANNED)
            throw new InvalidOperationException();
        Status = TrainingSessionStatus.CANCELLED;
        CancelledByUserId = actor;
        CancelledAtUtc = UpdatedAtUtc = now;
    }
    private static void ValidateTimes(DateTime? start, DateTime? end)
    {
        if (start is { Kind: not DateTimeKind.Utc } || end is { Kind: not DateTimeKind.Utc } || start.HasValue && end.HasValue && end <= start)
            throw new ArgumentOutOfRangeException(nameof(end));
    }
    private static string Required(string v, int m) => Optional(v, m) ?? throw new ArgumentException();
    private static string? Optional(string? v, int m)
    {
        if (string.IsNullOrWhiteSpace(v))
            return null;
        v = v.Trim();
        if (v.Length > m || v.Any(char.IsControl))
            throw new ArgumentOutOfRangeException(nameof(v));
        return v;
    }
}
