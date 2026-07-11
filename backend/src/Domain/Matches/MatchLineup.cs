using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class MatchLineup : Entity
{
    private MatchLineup() : base(Guid.Empty)
    {
    }

    private MatchLineup(Guid id, Guid matchId, string? formation, Guid? captainPlayerId, DateTime utcNow) : base(id)
    {
        MatchId = matchId;
        Formation = NormalizeFormation(formation);
        CaptainPlayerId = captainPlayerId;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchId { get; private set; }
    public string? Formation { get; private set; }
    public Guid? CaptainPlayerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static MatchLineup Create(Guid id, Guid matchId, string? formation, Guid? captainPlayerId, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(matchId, nameof(matchId));
        return new MatchLineup(id, matchId, formation, captainPlayerId, utcNow);
    }

    public void Update(string? formation, Guid? captainPlayerId, DateTime utcNow)
    {
        Formation = NormalizeFormation(formation);
        CaptainPlayerId = captainPlayerId;
        UpdatedAtUtc = utcNow;
    }

    private static string? NormalizeFormation(string? formation)
    {
        if (string.IsNullOrWhiteSpace(formation))
            return null;

        var normalized = string.Join(' ', formation.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > 32)
            throw new ArgumentOutOfRangeException(nameof(formation));

        return normalized;
    }
}
