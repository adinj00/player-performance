using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;
namespace PlayerPerformance.Domain.Training;

public sealed class TrainingSessionParticipant : Entity
{
    private TrainingSessionParticipant() : base(Guid.Empty)
    {
    }
    private TrainingSessionParticipant(Guid id, Guid s, Guid p, Guid a, DateTime n) : base(id)
    {
        TrainingSessionId = s;
        PlayerId = p;
        AddedByUserId = a;
        AddedAtUtc = n;
    }
    public Guid TrainingSessionId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid AddedByUserId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }
    public Guid? RemovedByUserId { get; private set; }
    public DateTime? RemovedAtUtc { get; private set; }
    public bool IsActive => RemovedAtUtc is null; public static TrainingSessionParticipant Create(Guid id, Guid s, Guid p, Guid a, DateTime n)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(s, nameof(s));
        Guard.AgainstDefault(p, nameof(p));
        Guard.AgainstDefault(a, nameof(a));
        return new(id, s, p, a, n);
    }
    public void Remove(Guid a, DateTime n)
    {
        Guard.AgainstDefault(a, nameof(a));
        if (!IsActive)
            throw new InvalidOperationException();
        RemovedByUserId = a;
        RemovedAtUtc = n;
    }
}
