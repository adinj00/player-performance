using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class MatchSubstitution : Entity
{
    private MatchSubstitution() : base(Guid.Empty)
    {
    }

    private MatchSubstitution(Guid id, Guid matchId, Guid playerOutId, Guid playerInId, int minute, int? stoppageTimeMinute, int sequence, DateTime utcNow) : base(id)
    {
        MatchId = matchId;
        PlayerOutId = playerOutId;
        PlayerInId = playerInId;
        Minute = minute;
        StoppageTimeMinute = stoppageTimeMinute;
        Sequence = sequence;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid PlayerOutId { get; private set; }
    public Guid PlayerInId { get; private set; }
    public int Minute { get; private set; }
    public int? StoppageTimeMinute { get; private set; }
    public int Sequence { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static MatchSubstitution Create(Guid id, Guid matchId, Guid playerOutId, Guid playerInId, int minute, int? stoppageTimeMinute, int sequence, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(matchId, nameof(matchId));
        EnsureValues(playerOutId, playerInId, minute, stoppageTimeMinute, sequence);
        return new MatchSubstitution(id, matchId, playerOutId, playerInId, minute, stoppageTimeMinute, sequence, utcNow);
    }

    private static void EnsureValues(Guid playerOutId, Guid playerInId, int minute, int? stoppageTimeMinute, int sequence)
    {
        Guard.AgainstDefault(playerOutId, nameof(playerOutId));
        Guard.AgainstDefault(playerInId, nameof(playerInId));
        if (playerOutId == playerInId || minute < 0 || stoppageTimeMinute < 0 || sequence <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequence));
    }
}
