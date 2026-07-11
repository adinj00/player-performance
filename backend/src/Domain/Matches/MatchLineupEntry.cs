using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class MatchLineupEntry : Entity
{
    private MatchLineupEntry() : base(Guid.Empty)
    {
    }

    private MatchLineupEntry(Guid id, Guid matchId, Guid playerId, MatchLineupRole role, DateTime utcNow) : base(id)
    {
        MatchId = matchId;
        PlayerId = playerId;
        Role = role;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid PlayerId { get; private set; }
    public MatchLineupRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static MatchLineupEntry Create(Guid id, Guid matchId, Guid playerId, MatchLineupRole role, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(matchId, nameof(matchId));
        Guard.AgainstDefault(playerId, nameof(playerId));
        EnsureRole(role);
        return new MatchLineupEntry(id, matchId, playerId, role, utcNow);
    }

    public void UpdateRole(MatchLineupRole role, DateTime utcNow)
    {
        EnsureRole(role);
        Role = role;
        UpdatedAtUtc = utcNow;
    }

    private static void EnsureRole(MatchLineupRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
    }
}
