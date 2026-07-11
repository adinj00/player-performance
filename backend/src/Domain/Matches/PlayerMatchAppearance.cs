using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class PlayerMatchAppearance : Entity
{
    private PlayerMatchAppearance() : base(Guid.Empty)
    {
    }

    private PlayerMatchAppearance(Guid id, Guid matchId, Guid playerId, int minutesPlayed, DateTime utcNow) : base(id)
    {
        MatchId = matchId;
        PlayerId = playerId;
        MinutesPlayed = minutesPlayed;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int MinutesPlayed { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static PlayerMatchAppearance Create(Guid id, Guid matchId, Guid playerId, int minutesPlayed, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(matchId, nameof(matchId));
        Guard.AgainstDefault(playerId, nameof(playerId));
        EnsureMinutes(minutesPlayed);
        return new PlayerMatchAppearance(id, matchId, playerId, minutesPlayed, utcNow);
    }

    public void UpdateMinutes(int minutesPlayed, DateTime utcNow)
    {
        EnsureMinutes(minutesPlayed);
        MinutesPlayed = minutesPlayed;
        UpdatedAtUtc = utcNow;
    }

    private static void EnsureMinutes(int minutesPlayed)
    {
        if (minutesPlayed < 0)
            throw new ArgumentOutOfRangeException(nameof(minutesPlayed));
    }
}
