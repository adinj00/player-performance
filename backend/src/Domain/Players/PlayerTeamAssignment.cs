using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Players;

public sealed class PlayerTeamAssignment : Entity
{
    private PlayerTeamAssignment() : base(Guid.Empty) { }

    private PlayerTeamAssignment(Guid id, Guid playerId, Guid teamId, DateOnly startDate, DateOnly? endDate, DateTime utcNow) : base(id)
    {
        PlayerId = playerId;
        TeamId = teamId;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid PlayerId { get; private set; }
    public Guid TeamId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static PlayerTeamAssignment Create(Guid id, Guid playerId, Guid teamId, DateOnly startDate, DateOnly? endDate, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(playerId, nameof(playerId));
        Guard.AgainstDefault(teamId, nameof(teamId));
        EnsureDateRange(startDate, endDate);
        return new PlayerTeamAssignment(id, playerId, teamId, startDate, endDate, utcNow);
    }

    public void End(DateOnly endDate, DateTime utcNow)
    {
        if (EndDate is not null)
            throw new InvalidOperationException("The player assignment has already ended.");
        EnsureDateRange(StartDate, endDate);
        EndDate = endDate;
        UpdatedAtUtc = utcNow;
    }

    private static void EnsureDateRange(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate is not null && endDate.Value < startDate)
            throw new ArgumentOutOfRangeException(nameof(endDate), "The assignment end date cannot be before its start date.");
    }
}
